﻿using Hexbear.Lib.EFCore;
using Hexbear.Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace Hexbear.API.Services
{
    public class UserService
    {
        public async Task<ListUsersResponse> ListUsers(LemmyContext db)
        {
            var users = await db.Persons.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync();
            return new ListUsersResponse()
            {
                Users = users,
            };
        }

        public async Task<UserResponse> GetUserByName(LemmyContext db, string username)
        {
            var person = await db.Persons.FirstOrDefaultAsync(x => x.Name == username);
            if (person == null)
                return null;
            var commentReports = await db.CommentReports.Where(x => x.Comment.CreatorId == person.Id).Select(x => new ReportItem()
            {
                CreatorName = x.Creator.Name,
                Reason = x.Reason,
                Resolved = x.Resolved,
                ResolverName = x.Resolver.Name,
                PosterName = x.Comment.Creator.Name,
                ReportType = "Comment",
                OriginalText = x.OriginalCommentText,
                DateCreated = x.Published,
            }).ToListAsync();
            var postReports = await db.PostReports.Where(x => x.Post.CreatorId == person.Id).Select(x => new ReportItem()
            {
                CreatorName = x.Creator.Name,
                Reason = x.Reason,
                Resolved = x.Resolved,
                ResolverName = x.Resolver.Name,
                PosterName = x.Post.Creator.Name,
                ReportType = "Post",
                OriginalText = x.OriginalPostBody,
                DateCreated = x.Published,
            }).ToListAsync();
            var dmReports = await db.PrivateMessageReports.Where(x => x.CreatorId == person.Id).Select(x => new ReportItem()
            {
                CreatorName = x.Creator.Name,
                Reason = x.Reason,
                Resolved = x.Resolved,
                ResolverName = x.Resolver.Name,
                PosterName = x.PrivateMessage.Creator.Name,
                ReportType = "Private Message",
                OriginalText = x.OriginalPmText,
                DateCreated = x.Published,
            }).ToListAsync();

            var commentReportsCreated = await db.CommentReports.Where(x => x.CreatorId == person.Id).Select(x => new ReportItem()
            {
                CreatorName = x.Creator.Name,
                Reason = x.Reason,
                Resolved = x.Resolved,
                ResolverName = x.Resolver.Name,
                PosterName = x.Comment.Creator.Name,
                ReportType = "Comment",
                OriginalText = x.OriginalCommentText,
                DateCreated = x.Published,
            }).ToListAsync();
            var postReportsCreated = await db.PostReports.Where(x => x.CreatorId == person.Id).Select(x => new ReportItem()
            {
                CreatorName = x.Creator.Name,
                Reason = x.Reason,
                Resolved = x.Resolved,
                ResolverName = x.Resolver.Name,
                PosterName = x.Post.Creator.Name,
                ReportType = "Post",
                OriginalText = x.OriginalPostBody,
                DateCreated = x.Published,
            }).ToListAsync();
            var dmReportsCreated = await db.PrivateMessageReports.Where(x => x.PrivateMessage.CreatorId == person.Id).Select(x => new ReportItem()
            {
                CreatorName = x.Creator.Name,
                Reason = x.Reason,
                Resolved = x.Resolved,
                ResolverName = x.Resolver.Name,
                PosterName = x.PrivateMessage.Creator.Name,
                ReportType = "Private Message",
                OriginalText = x.OriginalPmText,
                DateCreated = x.Published,
            }).ToListAsync();

            var upvotedRemovedPosts = (await db.PostLikes
                .Where(y => y.PersonId == person.Id)
                .Where(y => y.Post.Removed && y.Post.CreatorId != person.Id)
                .Select(y => y.Post).ToListAsync());
            var upvotedRemovedComments = (await db.CommentLikes
                .Where(y => y.PersonId == person.Id)
                .Where(y => y.Comment.Removed && y.Comment.CreatorId != person.Id)
                .Select(y => y.Comment).ToListAsync());

            return new UserResponse()
            {
                Person = person,
                ReportedItems = commentReports.Concat(postReports).Concat(dmReports).ToList(),
                ReportsCreatedItems = commentReportsCreated.Concat(postReportsCreated).Concat(dmReportsCreated).ToList(),
                UpvotedRemovedComments = upvotedRemovedComments,
                UpvotedRemovedPosts = upvotedRemovedPosts
            };
        }
    }
}
