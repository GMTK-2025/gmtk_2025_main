namespace Event
{
    public enum GameEventType
    {
        NONE,
        GAME_STAR,
        GAME_OVER,
    }

    public enum InputEventType
    {
        NONE,
        DEFAULT_DETECTION,
        SELECT_HOVER_DETECTION,
        SELECT_CLICK_DETECTION,
        SELECT_CANCEL,
        CONFIRM_CONFIRM,
        CONFIRM_CANCEL,
    }

    public enum PlayerEventType
    {
        PLAYER_CREATE,
        PLAYER_DELETE,
        EMPTY,
    }

    public enum NetworkEventType
    {
        START_CLIENT,
        START_SERVER,
        CLIENT_CONNECTED,
        CLIENT_DISCONNECTED,
        EMPTY,
    }

    public enum SceneEventType
    {
        MAIN_MENU_ENTER,
        MAIN_MENU_EXIT,
        TREASURE_ENTER,
        TREASURE_EXIT,
        EMPTY,
    }

    public enum AudioEventType
    {
    }

    public enum LobbyEventType
    {
        HOST_LOBBY,
        LOBBY_ENTER,
        LOBBY_MEMBER_CREATE,
        LOBBY_MEMBER_HOST_STATE_UPDATE,
        LOBBY_MEMBER_STEAM_NAME_UPDATE,
        LOBBY_MEMBER_CHARACTER_TYPE_UPDATE,
        LOBBY_MEMBER_READY_STATE_UPDATE,
        LOBBY_MEMBER_TEAM_COLOR_UPDATE,
        LOBBY_MEMBER_LEAVE,
        LOBBY_ROUND_LIMIT_UPDATE,
        LOBBY_DOMINATION_LIMIT_UPDATE,
        LOBBY_EXIT,
        EMPTY,
    }

    public enum CellEventType
    {
        CELL_CREATE,
        CELL_DELETE,
        CELL_INDEX_UPDATE,
        CELL_OWNERSHIP_UPDATE,
        CELL_COORD_UPDATE,
        CELL_POSITION_UPDATE,
        CELL_TYPE_UPDATE,
        CELL_COST_UPDATE,
        CELL_COLOR_FLAG_UPDATE,
        EMPTY,
    }

    public enum CharacterEventType
    {
        CHARACTER_CREATE,
        CHARACTER_DELETE,
        CHARACTER_TYPE_UPDATE,
        CHARACTER_COLOR_FLAG_UPDATE,
        CHARACTER_COIN_UPDATE,
        CHARACTER_DOMINATION_PERCENT_UPDATE,
        CHARACTER_DICE_TYPE_UPDATE,
        CHARACTER_COORD_UPDATE,
        CHARACTER_BEHAVIOUR_UPDATE,
        CHARACTER_TURN_STATE_UPDATE,
        CHARACTER_DICE_STATE_UPDATE,
        CHARACTER_DOMINATE_STATE_UPDATE,
        CHARACTER_ACTION_POINT_UPDATE,
        CHARACTER_HOLD_ITEM_UID_UPDATE,
        CHARACTER_HOLD_ITEM_UPDATE,
        CHARACTER_SELECT_CHARACTER_UPDATE,
        CHARACTER_DOMINATION_RANGE_UPDATE,
        EMPTY,
    }

    public enum ActivityEventType
    {
        ACTIVITY_CREATE,
        ACTIVITY_DELETE,
        EMPTY
    }

    public enum RoundEventType
    {
        ROUND_CREATE,
        ROUND_DELETE,
        ROUND_TYPE_UPDATE,
        ROUND_COUNT_UPDATE,
        ROUND_MAX_LIMIT_UPDATE,
        EMPTY
    }

    public enum StoreEventType
    {
        DEFAULT_STORE_ITEM_CREATE,
        DEFAULT_STORE_ITEM_TYPE_UPDATE,
        DEFAULT_STORE_ITEM_OWNED_UPDATE,
        DEFAULT_STORE_ITEM_UNLOCK_UPDATE,
        DEFAULT_STORE_ITEM_VISIBLE_UPDATE,
        DEFAULT_STORE_ITEM_INVENTORY_UPDATE,
        DEFAULT_STORE_ITEM_COST_UPDATE,
        DEFAULT_STORE_ITEM_DISCOUNT_UPDATE,
        DEFAULT_STORE_TIME_DELETE,
        ACTIVITY_STORE_ITEM_CREATE,
        ACTIVITY_STORE_ITEM_UPDATE,
        ACTIVITY_STORE_TIME_DELETE,
    }

    public enum BackpackEventType
    {
        BACKPACK_ITEM_CREATE,
        BACKPACK_ITEM_DELETE,
        BACKPACK_ITEM_TYPE_UPDATE,
        BACKPACK_ITEM_INDEX_UPDATE,
        BACKPACK_ITEM_OWNERSHIP_UPDATE,
        BACKPACK_ITEM_STACK_AMOUNT_UPDATE,
    }

    public enum ItemEventType
    {
    }

    public enum CameraEventType
    {
        EMPTY,
    }
}