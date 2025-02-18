﻿using AutomaticTypeMapper;
using EndlessClient.Input;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Map;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;

namespace EndlessClient.Controllers
{
    [MappedType(BaseType = typeof(IArrowKeyController))]
    public class ArrowKeyController : IArrowKeyController
    {
        private readonly IWalkValidationActions _walkValidationActions;
        private readonly ICharacterAnimationActions _characterAnimationActions;
        private readonly ICharacterProvider _characterProvider;
        private readonly IUnwalkableTileActions _unwalkableTileActions;
        private readonly ISpikeTrapActions _spikeTrapActions;

        public ArrowKeyController(IWalkValidationActions walkValidationActions,
                                  ICharacterAnimationActions characterAnimationActions,
                                  ICharacterProvider characterProvider,
                                  IUnwalkableTileActions walkErrorHandler,
                                  ISpikeTrapActions spikeTrapActions)
        {
            _walkValidationActions = walkValidationActions;
            _characterAnimationActions = characterAnimationActions;
            _characterProvider = characterProvider;
            _unwalkableTileActions = walkErrorHandler;
            _spikeTrapActions = spikeTrapActions;
        }

        public bool MoveLeft()
        {
            if (!CanWalkAgain())
                return false;

            FaceOrAttemptWalk(EODirection.Left);

            return true;
        }

        public bool MoveRight()
        {
            if (!CanWalkAgain())
                return false;

            FaceOrAttemptWalk(EODirection.Right);

            return true;
        }

        public bool MoveUp()
        {
            if (!CanWalkAgain())
                return false;

            FaceOrAttemptWalk(EODirection.Up);

            return true;
        }

        public bool MoveDown()
        {
            if (!CanWalkAgain())
                return false;

            FaceOrAttemptWalk(EODirection.Down);

            return true;
        }

        private bool CanWalkAgain()
        {
            return _characterProvider.MainCharacter.RenderProperties.IsActing(CharacterActionState.Standing) ||
                   _characterProvider.MainCharacter.RenderProperties.ActualWalkFrame == CharacterRenderProperties.MAX_NUMBER_OF_WALK_FRAMES - 1;
        }

        private bool CurrentDirectionIs(EODirection direction)
        {
            return _characterProvider.MainCharacter.RenderProperties.IsFacing(direction);
        }

        private void FaceOrAttemptWalk(EODirection direction)
        {
            if (!CurrentDirectionIs(direction) && _characterProvider.MainCharacter.RenderProperties.IsActing(CharacterActionState.Standing))
            {
                _characterAnimationActions.Face(direction);
            }
            else
            {
                _characterAnimationActions.Face(direction);
                AttemptToStartWalking();
            }
        }

        private void AttemptToStartWalking()
        {
            if (!_walkValidationActions.CanMoveToDestinationCoordinates())
            {
                _unwalkableTileActions.HandleUnwalkableTile();
            }
            else
            {
                _characterAnimationActions.StartWalking();

                var coordinate = new MapCoordinate(
                    _characterProvider.MainCharacter.RenderProperties.MapX,
                    _characterProvider.MainCharacter.RenderProperties.MapY);
                _spikeTrapActions.HideSpikeTrap(coordinate);

                coordinate = new MapCoordinate(
                    _characterProvider.MainCharacter.RenderProperties.GetDestinationX(),
                    _characterProvider.MainCharacter.RenderProperties.GetDestinationY());
                _spikeTrapActions.ShowSpikeTrap(coordinate);
            }
        }
    }

    public interface IArrowKeyController
    {
        bool MoveLeft();

        bool MoveRight();

        bool MoveUp();

        bool MoveDown();
    }
}
