using System;
using System.Collections.Generic;
using Mapping.Input;

namespace Event
{
    #region Base Event & Interface

    public abstract class BaseEventBus<TEventType> where TEventType : Enum
    {
        private readonly Dictionary<TEventType, IEventInfo> _eventDictionary = new();

        public void AddListener(TEventType name, Action method)
        {
            if (_eventDictionary.TryGetValue(name, out var value))
            {
                ((EventInfo)value).action += method;
            }
            else
            {
                var eventInfo = new EventInfo
                {
                    action = method
                };
                _eventDictionary.Add(name, eventInfo);
            }
        }

        public void AddListener<T>(TEventType name, Action<T> method)
        {
            if (_eventDictionary.TryGetValue(name, out var value))
            {
                ((EventInfo<T>)value).action += method;
            }
            else
            {
                var eventInfo = new EventInfo<T>
                {
                    action = method
                };
                _eventDictionary.Add(name, eventInfo);
            }
        }

        public void AddListener<T, U>(TEventType name, Action<T, U> method)
        {
            if (_eventDictionary.TryGetValue(name, out var value))
            {
                ((EventInfo<T, U>)value).action += method;
            }
            else
            {
                var eventInfo = new EventInfo<T, U>
                {
                    action = method
                };
                _eventDictionary.Add(name, eventInfo);
            }
        }

        public void RemoveListener(TEventType name, Action method)
        {
            if (_eventDictionary.TryGetValue(name, value: out var value))
            {
                ((EventInfo)value).action -= method;
            }
        }

        public void RemoveListener<T>(TEventType name, Action<T> method)
        {
            if (_eventDictionary.TryGetValue(name, out var value))
            {
                ((EventInfo<T>)value).action -= method;
            }
        }

        public void RemoveListener<T, U>(TEventType name, Action<T, U> method)
        {
            if (_eventDictionary.TryGetValue(name, value: out var value))
            {
                ((EventInfo<T, U>)value).action -= method;
            }
        }

        public void SendMessage(TEventType name)
        {
            if (_eventDictionary.TryGetValue(name, value: out var value))
            {
                ((EventInfo)value).action?.Invoke();
            }
        }

        public void SendMessage<T>(TEventType name, T paramT)
        {
            if (_eventDictionary.TryGetValue(name, value: out var value))
            {
                ((EventInfo<T>)value).action?.Invoke(paramT);
            }
        }

        public void SendMessage<T, U>(TEventType name, T paramT, U paramU)
        {
            if (_eventDictionary.TryGetValue(name, out var value))
            {
                ((EventInfo<T, U>)value).action?.Invoke(paramT, paramU);
            }
        }

        public void ClearEvent()
        {
            _eventDictionary.Clear();
        }
    }

    public interface IEventInfo
    {
    }

    public class EventInfo : IEventInfo
    {
        public Action action;
    }

    public class EventInfo<T> : IEventInfo
    {
        public Action<T> action;
    }

    public class EventInfo<T, U> : IEventInfo
    {
        public Action<T, U> action;
    }

    #endregion

    public class GameEventBus : BaseEventBus<GameEventType>
    {
    }

    public class InputEventBus : BaseEventBus<InputEventType>
    {
    }

    public class PlayerEventBus : BaseEventBus<PlayerEventType>
    {
    }

    public class NetworkEventBus : BaseEventBus<NetworkEventType>
    {
    }

    public class AudioEventBus : BaseEventBus<AudioEventType>
    {
    }

    public class LobbyEventBus : BaseEventBus<LobbyEventType>
    {
    }

    public class CellEventBus : BaseEventBus<CellEventType>
    {
    }

    public class SceneEventBus : BaseEventBus<SceneEventType>
    {
    }

    public class CharacterEventBus : BaseEventBus<CharacterEventType>
    {
    }

    public class InputStateEventBus : BaseEventBus<InputStateType>
    {
    }

    public class StoreEventBus : BaseEventBus<StoreEventType>
    {
    }

    public class BackpackEventBus : BaseEventBus<BackpackEventType>
    {
    }

    public class ActivityEventBus : BaseEventBus<ActivityEventType>
    {
    }

    public class RoundEventBus : BaseEventBus<RoundEventType>
    {
    }
    
    public class AnimationEventBus : BaseEventBus<AnimationEventType>
    {
    }

    public static class EventBus
    {
        private static BaseEventBus<GameEventType> _game;
        private static BaseEventBus<InputEventType> _input;
        private static BaseEventBus<BackpackEventType> _backpack;
        private static BaseEventBus<CharacterEventType> _character;
        private static BaseEventBus<InputStateType> _inputState;
        private static BaseEventBus<SceneEventType> _scene;
        private static BaseEventBus<CellEventType> _cell;
        private static BaseEventBus<LobbyEventType> _lobby;
        private static BaseEventBus<StoreEventType> _store;
        private static BaseEventBus<ActivityEventType> _activity;
        private static BaseEventBus<RoundEventType> _round;
        private static BaseEventBus<AnimationEventType> _animation;
        public static BaseEventBus<InputStateType> InputState =>
            _inputState ??= new InputStateEventBus();

        public static BaseEventBus<GameEventType> Game => _game ??= new GameEventBus();
        public static BaseEventBus<InputEventType> Input => _input ??= new InputEventBus();
        public static BaseEventBus<BackpackEventType> Backpack => _backpack ??= new BackpackEventBus();
        public static BaseEventBus<CharacterEventType> Character => _character ??= new CharacterEventBus();
        public static BaseEventBus<SceneEventType> Scene => _scene ??= new SceneEventBus();
        public static BaseEventBus<CellEventType> Cell => _cell ??= new CellEventBus();
        public static BaseEventBus<LobbyEventType> Lobby => _lobby ??= new LobbyEventBus();
        public static BaseEventBus<StoreEventType> Store => _store ??= new StoreEventBus();
        public static BaseEventBus<ActivityEventType> Activity => _activity ??= new ActivityEventBus();
        public static BaseEventBus<RoundEventType> Round => _round ??= new RoundEventBus();
        public static BaseEventBus<AnimationEventType> Animation => _animation ??= new AnimationEventBus();
    }
}