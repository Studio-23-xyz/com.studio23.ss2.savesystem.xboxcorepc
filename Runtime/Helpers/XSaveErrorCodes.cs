namespace Studio23.SS2.SaveSystem.XboxCorePc.Helpers
{
    public enum XSaveErrorCodes : uint
    {
        /// <summary>
        /// The name of the container is invalid.
        //Valid characters for the path portion of the container name (up to and including the final forward slash) includes uppercase letters (A-Z),
        //lowercase letters (a-z), numbers (0-9), underscore (_), and forward slash (/). The path portion may be empty.
        //Valid characters for the file name portion (everything after the final forward slash) include uppercase letters (A-Z),
        //lowercase letters (a-z), numbers (0-9), underscore (_), period (.), and hyphen (-). The file name may not be empty,
        //end in a period or contain two consecutive periods.
        /// </summary>
        E_GS_INVALID_CONTAINER_NAME = 0x80830001,

        /// <summary>
        /// The Xbox services Configuration ID (SCID) is not configured correctly on the service.
        /// </summary>
        E_GS_NO_ACCESS = 0x80830002,

        /// <summary>
        /// The device has run out of room for save games. 
        /// Users will need to free up storage space on the device. 
        /// This error can be hit even if the game's per user storage quota has not been exceeded.
        /// </summary>
        E_GS_OUT_OF_LOCAL_STORAGE = 0x80830003,


        /// <summary>
        /// The user canceled the download of their save games.
        /// </summary>
        E_GS_USER_CANCELED = 0x80830004,


        /// <summary>
        /// The size of the update is too big. 
        /// The total size of the update must be smaller than GS_MAX_BLOB_SIZE (16 MB),
        /// regardless of the total number of blobs.
        /// </summary>
        E_GS_UPDATE_TOO_BIG = 0x80830005,

        /// <summary>
        /// The game has exceeded the per-user quota for the game. 
        /// By default this quota is 256MB. Games can ask for an exception to make this larger.
        /// </summary>
        E_GS_QUOTA_EXCEEDED = 0x80830006,

        /// <summary>
        /// The buffer provided to the API was too small.
        /// </summary>
        E_GS_PROVIDED_BUFFER_TOO_SMALL = 0x80830007,

        /// <summary>
        /// The specified blob can't be found.
        /// </summary>
        E_GS_BLOB_NOT_FOUND = 0x80830008,

        /// <summary>
        /// The title is not properly configured for using connected storage. 
        /// This is possibly because the SCID is wrong or because this isn't configured correctly in Partner Center.
        /// </summary>
        E_GS_NO_SERVICE_CONFIGURATION = 0x80830009,

        /// <summary>
        /// The container is not yet synchronized.
        /// </summary>
        E_GS_CONTAINER_NOT_IN_SYNC = 0x8083000A,

        /// <summary>
        /// The synchronization of the container has failed.
        /// </summary>
        E_GS_CONTAINER_SYNC_FAILED = 0x8083000B,

        /// <summary>
        /// This indicates that the MSA isn't an Xbox services account yet.
        /// </summary>
        E_GS_USER_NOT_REGISTERED_IN_SERVICE = 0x8083000C,


        /// <summary>
        /// The handle used by the function has expired and should be reacquired.
        /// There are three handle types that are used by XGameSave: XGameSaveProviderHandle, XGameSaveContainerHandle, and XGameSaveUpdateHandle. 
        /// The XGameSaveUpdateHandle can't be re-used after submitting an update. Additionally, the XGameSaveUpdateHandle is no longer valid after the game is suspended.
        /// </summary>
        E_GS_HANDLE_EXPIRED = 0x8083000D,

        /// <summary>
        /// The function is getting called on a time-sensitive thread. This can cause deadlocks in the game. The caller should use an asynchronous version of the API. For more information, see Time-sensitive threads.
        /// </summary>
        E_GS_ASYNC_FUNCTION_REQUIRED = 0x8083000E,

        /// <summary>
        /// This error is never returned to the title through any of the user-facing XGameSave APIs. Instead, developers may see this in their debug output at the time that their game is terminated. This error indicates that the title didn't have the connected storage lock at the time the game save provider was initialized. This could have happened for several reasons including being offline at that time or having the user choose to play offline when presented with a conflict dialog. As the game didn't have the connected storage lock, the OS will terminate the game and present this error when it goes to suspend so as to get the game into a good state for the next launch.
        /// </summary>
        E_GS_TERMINATEDTITLE_STALE_DATA = 0x80831001,

        /// <summary>
        /// The game is trying to mix and match the usage of XGameSave and XGameSaveFiles. This is not supported.
        /// Games much choose which cloud save system they want to use in their game.
        /// </summary>
        E_GS_PROVIDER_MISMATCH = 0x8083000F

    }
}