﻿namespace LabSolution.Notifications
{
    public class AppEmailNotificationConfig
    {
        public bool SendNotificationForOnlineBooking { get; set; }
        public bool SendNotificationForInHouseBooking { get; set; }
        public bool SendNotificationWhenTestIsCompleted { get; set; }
    }
}
