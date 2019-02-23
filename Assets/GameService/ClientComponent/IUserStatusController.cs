namespace GameService.ClientComponent {
    public enum UserStatus {
        LOADING, GAMING, OFFLINE, CONN_LOST
    }
    
    public interface IUserStatusController {
        void ChangeStatus(UserStatus status);
        void SetLoadingProgress(float percent);
        
    }
}