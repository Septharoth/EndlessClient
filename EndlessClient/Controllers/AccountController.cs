﻿using System.Threading.Tasks;
using AutomaticTypeMapper;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Domain.Account;
using EOLib.Net;
using EOLib.Net.Communication;
using XNAControls;

namespace EndlessClient.Controllers
{
    [MappedType(BaseType = typeof(IAccountController))]
    public class AccountController : IAccountController
    {
        private readonly IAccountDialogDisplayActions _accountDialogDisplayActions;
        private readonly IErrorDialogDisplayAction _errorDisplayAction;
        private readonly IAccountActions _accountActions;
        private readonly IGameStateActions _gameStateActions;
        private readonly ISafeNetworkOperationFactory _networkOperationFactory;

        public AccountController(IAccountDialogDisplayActions accountDialogDisplayActions,
                                 IErrorDialogDisplayAction errorDisplayAction,
                                 IAccountActions accountActions,
                                 IGameStateActions gameStateActions,
                                 ISafeNetworkOperationFactory networkOperationFactory)
        {
            _accountDialogDisplayActions = accountDialogDisplayActions;
            _errorDisplayAction = errorDisplayAction;
            _accountActions = accountActions;
            _gameStateActions = gameStateActions;
            _networkOperationFactory = networkOperationFactory;
        }

        public async Task CreateAccount(ICreateAccountParameters createAccountParameters)
        {
            var paramsValidationResult = _accountActions.CheckAccountCreateParameters(createAccountParameters);
            if (paramsValidationResult.FaultingParameter != WhichParameter.None)
            {
                _accountDialogDisplayActions.ShowCreateParameterValidationError(paramsValidationResult);
                return;
            }

            var checkNameOperation = _networkOperationFactory.CreateSafeBlockingOperation(
                async () => await _accountActions.CheckAccountNameWithServer(createAccountParameters.AccountName),
                SetInitialStateAndShowError,
                SetInitialStateAndShowError);
            if (!await checkNameOperation.Invoke())
                return;

            var nameResult = checkNameOperation.Result;
            if (nameResult < AccountReply.OK_CodeRange)
            {
                _accountDialogDisplayActions.ShowCreateAccountServerError(nameResult);
                return;
            }

            var result = await _accountDialogDisplayActions.ShowCreatePendingDialog();
            if (result == XNADialogResult.Cancel)
                return;

            var createAccountOperation = _networkOperationFactory.CreateSafeBlockingOperation(
                async () => await _accountActions.CreateAccount(createAccountParameters),
                SetInitialStateAndShowError,
                SetInitialStateAndShowError);
            if (!await createAccountOperation.Invoke())
                return;

            var accountResult = createAccountOperation.Result;
            if (accountResult != AccountReply.Created)
            {
                _accountDialogDisplayActions.ShowCreateAccountServerError(accountResult);
                return;
            }

            _gameStateActions.ChangeToState(GameStates.Initial);
            _accountDialogDisplayActions.ShowSuccessMessage();
        }

        public async Task ChangePassword()
        {
            var changePasswordParameters = await _accountDialogDisplayActions.ShowChangePasswordDialog();
            if (!changePasswordParameters.HasValue)
                return;

            var changePasswordOperation = _networkOperationFactory.CreateSafeBlockingOperation(
                async () => await _accountActions.ChangePassword(changePasswordParameters.Value),
                SetInitialStateAndShowError,
                SetInitialStateAndShowError);
            if (!await changePasswordOperation.Invoke())
                return;

            var result = changePasswordOperation.Result;
            _accountDialogDisplayActions.ShowCreateAccountServerError(result);
        }

        private void SetInitialStateAndShowError(NoDataSentException ex)
        {
            _gameStateActions.ChangeToState(GameStates.Initial);
            _errorDisplayAction.ShowException(ex);
        }

        private void SetInitialStateAndShowError(EmptyPacketReceivedException ex)
        {
            _gameStateActions.ChangeToState(GameStates.Initial);
            _errorDisplayAction.ShowException(ex);
        }
    }

    public interface IAccountController
    {
        Task CreateAccount(ICreateAccountParameters createAccountParameters);

        Task ChangePassword();
    }
}