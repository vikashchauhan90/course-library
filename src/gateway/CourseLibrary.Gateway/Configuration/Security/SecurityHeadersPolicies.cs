using NetEscapades.AspNetCore.SecurityHeaders.Infrastructure;

namespace CourseLibrary.Gateway.Configuration.Security;


public class SecurityHeadersPolicies
{

    public static HeaderPolicyCollection AddCourseLibraryDefaultSecurityHeaders()
    {
        return new HeaderPolicyCollection()
            .AddFrameOptionsDeny()
            .AddXssProtectionBlock()
            .AddContentTypeOptionsNoSniff()
            .AddReferrerPolicyOriginWhenCrossOrigin()
            .AddPermissionsPolicy(builder =>
            {
                builder.AddCamera().None();
                builder.AddMicrophone().None();
                builder.AddGeolocation().None();
                builder.AddPayment().None();
                builder.AddUsb().None();
            })
            .AddContentSecurityPolicy(builder =>
            {
                builder.AddDefaultSrc().Self();
                builder.AddScriptSrc().Self();
                builder.AddStyleSrc().Self();
                builder.AddImgSrc().Self().Data();
                builder.AddFontSrc().Self();
                builder.AddConnectSrc().Self();
                builder.AddObjectSrc().None();
                builder.AddBaseUri().Self();
                builder.AddFrameAncestors().None();
                builder.AddFormAction().Self();
                builder.AddUpgradeInsecureRequests();
            });
    }
}
