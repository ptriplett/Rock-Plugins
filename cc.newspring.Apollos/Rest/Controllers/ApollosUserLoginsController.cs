﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using cc.newspring.Apollos.Utilities;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Controllers;
using Rock.Rest.Filters;

namespace cc.newspring.Apollos.Rest.Controllers
{
    public class ApollosUserLoginsController : UserLoginsController
    {
        private string UserNameKey = "UserName";
        private string HashKey = "ApollosHash";
        private string ApollosAuthName = "cc.newspring.Apollos.Security.Authentication.Apollos";

        /// <summary>
        /// Returns a single ApollosUserLogin
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/UserLogins/{id}" )]
        public ApollosUserLogin GetById( int id )
        {
            return new ApollosUserLogin( base.GetById( id ) );
        }

        /// <summary>
        /// Returns all ApollosUserLogins
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/UserLogins" )]
        public object[] Get()
        {
            var logins = base.Get();
            var apollosLogins = new Queue();

            foreach ( var login in logins )
            {
                apollosLogins.Enqueue( new ApollosUserLogin( login ) );
            }

            return apollosLogins.ToArray();
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        [HttpDelete]
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/UserLogins/{id}" )]
        public void Delete( int id )
        {
            base.Delete( id );
        }

        /// <summary>
        /// Updates the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        [HttpPost]
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/UserLogins/{id}" )]
        public HttpResponseMessage Update( int id, [FromBody]Dictionary<string, string> data )
        {
            var response = new HttpResponseMessage();
            var context = new RockContext();
            var userLoginService = new UserLoginService( context );
            var userLogin = userLoginService.Get( id );
            var changes = false;

            if ( userLogin == null )
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Content = new StringContent( string.Format( "There is no UserLogin that employs id {0}", id ) );
                return response;
            }

            if ( data.ContainsKey( UserNameKey ) )
            {
                var userName = data[UserNameKey];

                if ( !Validation.IsEmail( userName ) )
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Content = new StringContent( string.Format( "{0} must be a valid email", UserNameKey ) );
                    return response;
                }

                var existing = userLoginService.GetByUserName( userName );

                if ( existing != null && existing.Id != id )
                {
                    response.StatusCode = HttpStatusCode.Conflict;
                    response.Content = new StringContent( string.Format( "UserLogin with id={0} employs username {1}", existing.Id, existing.UserName ) );
                    return response;
                }

                changes = true;
                userLogin.UserName = userName;
            }

            if ( data.ContainsKey( HashKey ) )
            {
                var hash = data[HashKey];

                if ( !Validation.IsBcryptHash( hash ) )
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Content = new StringContent( string.Format( "{0} must be a valid Bcrypt hash", HashKey ) );
                    return response;
                }

                changes = true;
                userLogin.Password = hash;
            }

            if ( changes )
            {
                context.SaveChanges();
            }

            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        /// <summary>
        /// Inserts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        [HttpPost]
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/UserLogins" )]
        public object Post( [FromBody]Dictionary<string, string> data )
        {
            var response = new HttpResponseMessage();

            if ( !data.ContainsKey( UserNameKey ) )
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Content = new StringContent( string.Format( "Data must contain {0} ", UserNameKey ) );
                return response;
            }

            if ( !data.ContainsKey( HashKey ) )
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Content = new StringContent( string.Format( "Data must contain {0} ", HashKey ) );
                return response;
            }

            var userName = data[UserNameKey];
            var hash = data[HashKey];

            if ( !Validation.IsEmail( userName ) )
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Content = new StringContent( string.Format( "{0} must be a valid email", UserNameKey ) );
                return response;
            }

            if ( !Validation.IsBcryptHash( hash ) )
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Content = new StringContent( string.Format( "{0} must be a valid Bcrypt hash", HashKey ) );
                return response;
            }

            var context = new RockContext();
            var userLoginService = new UserLoginService( context );
            var existing = userLoginService.GetByUserName( userName );

            if ( existing != null )
            {
                response.StatusCode = HttpStatusCode.Conflict;
                response.Content = new StringContent( string.Format( "UserLogin with id={0} employs username {1}", existing.Id, existing.UserName ) );
                return response;
            }

            var entityTypeService = new EntityTypeService( context );
            var apollosAuth = entityTypeService.Get( ApollosAuthName );

            if ( apollosAuth == null )
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent( "Cannot find the Apollos Auth Entity" );
                return response;
            }

            var userLogin = new UserLogin
            {
                EntityTypeId = apollosAuth.Id,
                Password = hash,
                UserName = userName
            };

            userLoginService.Add( userLogin );
            context.SaveChanges();

            return new ApollosUserLogin( userLogin );
        }
    }
}