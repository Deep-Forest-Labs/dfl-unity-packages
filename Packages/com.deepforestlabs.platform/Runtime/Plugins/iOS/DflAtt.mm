#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <Foundation/Foundation.h>

typedef void (*DFLAttCallback)(int status);

extern "C" int DFL_GetTrackingAuthorizationStatus(void)
{
    if (@available(iOS 14, *))
    {
        return (int)[ATTrackingManager trackingAuthorizationStatus];
    }

    // Pre-iOS 14: treat as authorized for ATT purposes (no prompt API).
    return 3;
}

extern "C" void DFL_RequestTrackingAuthorization(DFLAttCallback callback)
{
    if (callback == NULL)
    {
        return;
    }

    if (@available(iOS 14, *))
    {
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
            dispatch_async(dispatch_get_main_queue(), ^{
                callback((int)status);
            });
        }];
        return;
    }

    callback(3);
}
