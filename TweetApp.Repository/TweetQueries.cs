using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using TweetApp.Repository.TweetAppEntity;

namespace TweetApp.Repository
{
    public class TweetQueries : ITweetQueries
    {
        private readonly IConfiguration config;
        /// <summary>
        /// Checks the user is exists or not.
        /// </summary>
        /// <param name="emailId"> based on userID.</param>
        /// <returns>returns the user details if found.</returns>
        public TweetQueries(IConfiguration _config)
        {
            this.config = _config;
        }

        public string UserExist(string emailId)
        {
                var dbContext = new MongoClient(config.GetConnectionString("TweetAppCon"));          
                var filter = Builders<BsonDocument>.Filter.Eq("EmailId", emailId);
                var user = dbContext.GetDatabase("TweetAppDb").GetCollection<BsonDocument>("Users").Find(filter).FirstOrDefault();//.Users.Where(s => s.EmailId == emailId).Select(p => p.EmailId).FirstOrDefault();
            if (user == null)
            {
                return null;
            }
            return user.ToString();
        }

        /// <summary>
        /// user login.
        /// </summary>
        /// <param name="userId">based on user id fetches the encoded password</param>
        /// <returns></returns>
        public User Userlogin(string userId)
        {
            var dbContext = new MongoClient(config.GetConnectionString("TweetAppCon"));
            var filter = Builders<User>.Filter.Eq("EmailId", userId);
            var user = dbContext.GetDatabase("TweetAppDb").GetCollection<User>("Users").Find(filter).FirstOrDefault();//.Users.Where(s => s.EmailId == emailId).Select(p => p.EmailId).FirstOrDefault();
            return user;
        }

        /// <summary>
        /// registered the new user
        /// </summary>
        /// <param name="userRegister">user details.</param>
        public bool UserRegister(User userRegister)
        {
            try
            {
                var dbContext = new MongoClient(config.GetConnectionString("TweetAppCon"));
                dbContext.GetDatabase("TweetAppDb").GetCollection<User>("Users").InsertOne(userRegister);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// adds the new tweet.
        /// </summary>
        /// <param name="tweet">tweet</param>
        public bool AddTweet(Tweet tweet)
        {
            try
            {
                var dbContext = new MongoClient(config.GetConnectionString("TweetAppCon"));
                dbContext.GetDatabase("TweetAppDb").GetCollection<Tweet>("Tweet").InsertOne(tweet);
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// get the particular user tweets.
        /// </summary>
        /// <param name="userId">userId.</param>
        /// <returns>returns the list of tweets.</returns>
        public List<TweetsandUsers> GetUserTweets(string userId)
        {
            var dbContext = new MongoClient(config.GetConnectionString("TweetAppCon"));
            var users = dbContext.GetDatabase("TweetAppDb").GetCollection<User>("Users");
            var filter = Builders<User>.Filter.Eq("EmailId", userId);
            var tweets = dbContext.GetDatabase("TweetAppDb").GetCollection<Tweet>("Tweet");
            var tweetsDoc = users.Aggregate().Match(filter).Lookup("Tweet", "EmailId", "UserId", "tweetsList").Project(new BsonDocument { { "_id", 1 }, { "FirstName", 1 }, { "ImgName", 1 } , { "tweetsList", 1 } }).ToList();
                List<TweetsandUsers> twtLst = new List<TweetsandUsers>();
            foreach (var item1 in tweetsDoc)
            {
                foreach (var item2 in item1.GetValue("tweetsList").AsBsonArray)
                {
                    TweetsandUsers obj = new TweetsandUsers()
                    {
                        Id = (int)item2[0],
                        UserId = item2[1].AsString,
                        FirstName = item1.GetValue("FirstName").AsString,
                        CreatedDate = Convert.ToDateTime(item2[4]),
                        ImgName = item1.GetValue("ImgName").AsString,
                        UserTweets = item2[3].AsString,
                        Likes = (int)item2[5],
                        visible = false
                    };
                    twtLst.Add(obj);
                }
            }
                return twtLst;
           
        }


        /// <summary>
        /// get the particular user tweets.
        /// </summary>
        /// <param name="userId">userId.</param>
        /// <returns>returns the list of tweets.</returns>
        public List<TweetsandUsers> GetOtherUserTweets(string userId)
        {
            var dbContext = new MongoClient(config.GetConnectionString("TweetAppCon"));
            var users = dbContext.GetDatabase("TweetAppDb").GetCollection<User>("Users");
            var tweets = dbContext.GetDatabase("TweetAppDb").GetCollection<Tweet>("Tweet");
            var filter = Builders<User>.Filter.Where(x=>x.EmailId!=userId);
          
            var tweetsDoc = users.Aggregate().Match(filter).Lookup("Tweet", "EmailId", "UserId", "tweetsList").Project(new BsonDocument { { "FirstName", 1 }, { "ImgName", 1 }, { "tweetsList", 1 } }).ToList();
            List<TweetsandUsers> twtLst = new List<TweetsandUsers>();
            foreach (var item1 in tweetsDoc)
            {
                foreach (var item2 in item1.GetValue("tweetsList").AsBsonArray)
                {
                    TweetsandUsers obj = new TweetsandUsers()
                    {
                        Id = (int)item2[0],
                        UserId = item2[1].AsString,
                        FirstName = item1.GetValue("FirstName").AsString,
                        CreatedDate = Convert.ToDateTime(item2[4]),
                        ImgName = item1.GetValue("ImgName").AsString,
                        UserTweets = item2[3].AsString,
                        Likes = (int)item2[5],
                        visible = false
                    };
                    twtLst.Add(obj);
                }
            }
            return twtLst;
        }

        /// <summary>
        /// Get all the user list.
        /// </summary>
        /// <returns>returns the all user list.</returns>
        public List<AllUsers> GetAllUsers()
        {
            var dbContext = new MongoClient(config.GetConnectionString("TweetAppCon"));
            var users = dbContext.GetDatabase("TweetAppDb").GetCollection<BsonDocument>("Users").AsQueryable().Select(x=>x);
            List<AllUsers> allUsers = new List<AllUsers>();
            foreach (var u in users)
            {                
                allUsers.Add(new AllUsers
                {
                    FirstName = u.GetValue("FirstName").AsString,
                    LastName = u.GetValue("LastName").AsString

                });
            }
            return allUsers;
            }

        /// <summary>
        /// Get all users and their respective Tweets.
        /// </summary>
        /// <returns>Returns the list users names and tweets.</returns>
            public List<TweetsandUsers> GetUserandTweetList()
        {
            var mongoDbClient = new MongoClient(config.GetConnectionString("TweetAppCon"));
            var tweets = mongoDbClient.GetDatabase("TweetAppDb").GetCollection<Tweet>("Tweet");
            var users= mongoDbClient.GetDatabase("TweetAppDb").GetCollection<User>("Users");

            var tweetsdocs = users.Aggregate().Lookup("Tweet", "EmailId", "UserId", "tweetList").Project(new BsonDocument {  { "FirstName", 1 }, { "CreatedDate", 1 }, { "ImgName", 1 }, { "tweetList", 1 } }).ToList();
            List<TweetsandUsers> userTweets = new List<TweetsandUsers>();
            foreach(var item1 in tweetsdocs)
            {
                foreach(var item2 in item1.GetValue("tweetList").AsBsonArray)
                {
                    TweetsandUsers obj = new TweetsandUsers()
                    {
                        UserId = item2[1].AsString,
                        FirstName = item1.GetValue("FirstName").AsString,
                        CreatedDate = Convert.ToDateTime(item2[4]),
                        ImgName = item1.GetValue("ImgName").AsString,
                        UserTweets = item2[3].AsString,
                        visible = false
                    };
                    userTweets.Add(obj);
                }
            }
           return userTweets;
        }
        /// <summary>
        /// updates the password.
        /// </summary>
        /// <param name="userId">Userid.</param>
        /// <param name="newPassword">Newpassword.</param>
        /// <returns></returns>

        public bool UpdatePassword(string userId, string oldPassword, string newPassword)
        {
            var mongoDbClient = new MongoClient(config.GetConnectionString("TweetAppCon"));
            var users = mongoDbClient.GetDatabase("TweetAppDb").GetCollection<User>("Users");
            var filter = Builders<User>.Filter.Where(x=>x.EmailId==userId && x.Password == oldPassword);
            var update = Builders<User>.Update.Set(x => x.Password, newPassword);
            var result=users.UpdateOne(filter,update);
            if(result.ModifiedCount>0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Forgotpassword email.
        /// </summary>
        /// <param name="emailId">based on emaild.</param>
        /// <returns>return the status.</returns>
        public bool ForgotPasswordEmail(string emailId)
        {
            var mongoDbClient = new MongoClient(config.GetConnectionString("TweetAppCon"));
            var users = mongoDbClient.GetDatabase("TweetAppDb").GetCollection<User>("Users");
            var filter = Builders<User>.Filter.Eq("EmailId",emailId );
            var userDetails = users.Find(filter).FirstOrDefault();
            if(userDetails==null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Forgot password.
        /// </summary>
        /// <param name="emailId">based on emailId.</param>
        /// <param name="newPassword">based on new password.</param>
        /// <returns>returns the status.</returns>
        public bool ForgotPassword(string emailId, string newPassword)
        {
            var mongoDbClient = new MongoClient(config.GetConnectionString("TweetAppCon"));
            var users = mongoDbClient.GetDatabase("TweetAppDb").GetCollection<User>("Users");
            var filter = Builders<User>.Filter.Where(x => x.EmailId == emailId);
            var update = Builders<User>.Update.Set(x => x.Password,newPassword);
            var result = users.UpdateOne(filter, update);
            if (result.ModifiedCount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Add new tweet comments.
        /// </summary>
        /// <param name="comments">comment need to add</param>
        /// <returns>return the status.</returns>
        public bool AddTweetComments(AddTweetComments comments, int tweetId)
        {
            var mongoDbClient = new MongoClient(config.GetConnectionString("TweetAppCon"));
            var tweetcomment = mongoDbClient.GetDatabase("TweetAppDb").GetCollection<TweetComment>("TweetComments");
            var addComment = new TweetComment()
            {
                UserId = comments.UserId,
                TweetId = tweetId,
                Comments = comments.Comments
            };
            try
            {
                tweetcomment.InsertOne(addComment);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Fetches the tweet comments.
        /// </summary>
        /// <param name="tweetId">Based on tweetId</param>
        /// <returns>returns the list of tweet comments.</returns>
        public List<CommentsOnTweet> FetchTweetComments(int tweetId)
        {
            var mongoDbClient = new MongoClient(config.GetConnectionString("TweetAppCon"));
            var tweetcomments = mongoDbClient.GetDatabase("TweetAppDb").GetCollection<TweetComment>("TweetComments");
            var tweets = mongoDbClient.GetDatabase("TweetAppDb").GetCollection<Tweet>("Tweet");
            var filter = Builders<Tweet>.Filter.Eq("_id", tweetId);
            var userTweets = new List<CommentsOnTweet>();
            
            var test = tweets.Aggregate().Match(filter).Lookup("TweetComments","_id", "TweetId","comments").Project(new BsonDocument {{ "comments", 1 }, { "UserId", 1 }, { "UserTweets", 1 } }).ToList();
                 List<CommentsOnTweet> usercomments = new List<CommentsOnTweet>();
            foreach (var item1 in test)
            {
                foreach(var item2 in item1.GetValue("comments").AsBsonArray)
                {
                    CommentsOnTweet obj = new CommentsOnTweet()
                    {
                        TweetId = (int)item2[1],
                        UserId = item1.GetValue("UserId").ToString(),
                        Comments = item2[3].AsString,
                        TweetPost = item1.GetValue("UserTweets").AsString
                    };
                    usercomments.Add(obj);
                }
            }
            return usercomments;
                            }

        /// <summary>
        /// Updates te likes.
        /// </summary>
        /// <param name="tweetId">based on tweetId.</param>
        /// <returns>return the status.</returns>
            public bool UpdateLikes(int tweetId)
        {      
            var mongoClient = new MongoClient(config.GetConnectionString("TweetAppCon"));
            var tweets = mongoClient.GetDatabase("TweetAppDb").GetCollection<Tweet>("Tweet");           
            var filter =Builders<Tweet>.Filter.Eq("Id", tweetId);
            var updatedLikes = tweets.AsQueryable().
                                Where(x => x.TweetId == tweetId).FirstOrDefault();                              
            var update = Builders<Tweet>.Update.Set("Likes", updatedLikes.Likes+1);
            var updateResult = tweets.UpdateOne(filter,update);
            if (updateResult.ModifiedCount>0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Likes and Dislikes Count.
        /// </summary>
        /// <param name="tweetId">based on tweetId.</param>
        /// <returns>returns the tweet.</returns>
        public List<TweetsandUsers> LikesandDislikesCount(int TweetId)
        {
            var mongoClient = new MongoClient(config.GetConnectionString("TweetAppCon"));
            var users = mongoClient.GetDatabase("TweetAppDb").GetCollection<User>("Users");
            var tweets = mongoClient.GetDatabase("TweetAppDb").GetCollection<Tweet>("Tweet");
            var filter = Builders<Tweet>.Filter.Where(x => x.TweetId == TweetId);
            var likesCountDoc = tweets.Aggregate().Match(filter).Lookup("Users", "UserId", "EmailId", "likeList").Project(new BsonDocument { { "UserId", 1 }, { "CreatedDate", 1 }, { "UserTweets", 1 }, { "Likes", 1 }, { "DisLikes", 1 }, { "likeList", 1 } }).ToList();
            List<TweetsandUsers> userLikeList = new List<TweetsandUsers>();
            foreach (var item1 in likesCountDoc)
            {
                foreach (var item2 in item1.GetValue("likeList").AsBsonArray)
                {
                    TweetsandUsers obj = new TweetsandUsers()
                    {
                        UserId = item1.GetValue("UserId").AsString,
                        FirstName = item2[1].AsString,
                        CreatedDate = Convert.ToDateTime(item1.GetValue("CreatedDate")),
                        ImgName = item2[7].AsString,
                        UserTweets = item1.GetValue("UserTweets").AsString,
                        Likes = (int)item1.GetValue("Likes"),
                        DisLikes= (int)item1.GetValue("DisLikes")
                    };
                    userLikeList.Add(obj);
                }
               
            }
            return userLikeList;
        }

            /// <summary>
            /// UpdateDislikes.
            /// </summary>
            /// <param name="tweetId">BasedontweetId.</param>
            /// <returns>return the status.</returns>
            public bool UpdateDisLikes(int tweetId)
        {
            var mongoClient = new MongoClient(config.GetConnectionString("TweetAppCon"));
            var tweets = mongoClient.GetDatabase("TweetAppDb").GetCollection<Tweet>("Tweet");
            var filter = Builders<Tweet>.Filter.Eq("Id", tweetId);
            var updatedDislikes = tweets.AsQueryable().
                                Where(x => x.TweetId == tweetId).FirstOrDefault();
            var update = Builders<Tweet>.Update.Set("DisLikes", updatedDislikes.DisLikes + 1);
            var updateResult = tweets.UpdateOne(filter, update);
            if (updateResult.ModifiedCount > 0)
            {
                return true;
            }
            return false;
        }

    }
}
