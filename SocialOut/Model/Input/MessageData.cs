﻿namespace SocialOut.Model.Input
{
    public class MessageData
    {
        public int channel { get; set; }

        public string receiveId { get; set; }

        public string senderId { get; set; }

        public string text { get; set; }

        public string appId { get; set; }
    }
}
