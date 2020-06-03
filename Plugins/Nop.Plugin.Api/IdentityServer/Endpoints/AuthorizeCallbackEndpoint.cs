namespace Nop.Plugin.Api.IdentityServer.Endpoints
{
    using Microsoft.AspNetCore.Http;
    using System.Net;
    using System.Threading.Tasks;

    public class AuthorizeCallbackEndpoint : AuthorizeEndpointBase
    {
        //public AuthorizeCallbackEndpoint()
        //{

        //}
        //return null;
        //private readonly IConsentMessageStore _consentResponseStore;

        //public AuthorizeCallbackEndpoint(
        //    IEventService events,
        //    IAuthorizeRequestValidator validator,
        //    IAuthorizeInteractionResponseGenerator interactionGenerator,
        //    IAuthorizeResponseGenerator authorizeResponseGenerator,
        //    IUserSession userSession,
        //    IConsentMessageStore consentResponseStore) : base(events, userSession, validator, authorizeResponseGenerator, interactionGenerator)
        //{
        //    _consentResponseStore = consentResponseStore;
        //}

        //public override async Task<IEndpointResult> ProcessAsync(HttpContext context)
        //{
        //    if (context.Request.Method != "GET")
        //    {
        //        return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        //    }
            
        //    var parameters = context.Request.Query.AsNameValueCollection();

        //    var user = await UserSession.GetUserAsync();
        //    var consentRequest = new ConsentRequest(parameters, user?.GetSubjectId());
        //    var consent = await _consentResponseStore.ReadAsync(consentRequest.Id);

        //    if (consent != null && consent.Data == null)
        //    {
        //        return await CreateErrorResultAsync("consent message is missing data");
        //    }

        //    try
        //    {
        //        var result = await ProcessAuthorizeRequestAsync(parameters, user, consent?.Data);
                
        //        return result;
        //    }
        //    finally
        //    {
        //        if (consent != null)
        //        {
        //            await _consentResponseStore.DeleteAsync(consentRequest.Id);
        //        }
        //    }
        //}
    }
}