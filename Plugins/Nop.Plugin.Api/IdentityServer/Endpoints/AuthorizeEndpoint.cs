//namespace Nop.Plugin.Api.IdentityServer.Endpoints
//{
//    using Microsoft.AspNetCore.Http;
//    using System.Collections.Specialized;
//    using System.Net;
//    using System.Threading.Tasks;

//    public class AuthorizeEndpoint : AuthorizeEndpointBase
//    {
//        public AuthorizeEndpoint(
//            IEventService events,
//            IAuthorizeRequestValidator validator,
//            IAuthorizeInteractionResponseGenerator interactionGenerator,
//            IAuthorizeResponseGenerator authorizeResponseGenerator,
//            IUserSession userSession) 
//            : base(events, userSession, validator, authorizeResponseGenerator, interactionGenerator)
//        {
//        }

//        public override async Task<IEndpointResult> ProcessAsync(HttpContext context)
//        {
//            NameValueCollection values;

//            if (context.Request.Method == "GET")
//            {
//                values = context.Request.Query.AsNameValueCollection();
//            }
//            else if (context.Request.Method == "POST")
//            {
//                if (!context.Request.HasFormContentType)
//                {
//                    return new StatusCodeResult(HttpStatusCode.UnsupportedMediaType);
//                }

//                values = context.Request.Form.AsNameValueCollection();
//            }
//            else
//            {
//                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
//            }

//            var user = await UserSession.GetUserAsync();
//            var result = await ProcessAuthorizeRequestAsync(values, user, null);
            
//            return result;
//        }
//    }
//}