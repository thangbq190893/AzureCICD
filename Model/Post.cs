﻿namespace Webhook.Model
{
    public class Post : FeedBase
    {
        public string PostId { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
        public string PhotoId { get; set; }
        public Post(string postId, string message, string link, string photoId, string senderName, string senderId, long time, string item) : base(senderId, senderName, time, item)
        {
            this.PostId = postId;
            this.Message = message;
            this.Link = link;
            this.PhotoId = photoId;
        }
    }
}
