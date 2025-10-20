#import <UIKit/UIKit.h>
#import "UnityAppController.h"

extern "C" void iosShare_sheet(const char* imagePath, const char* message)
{
    NSString* path = imagePath ? [NSString stringWithUTF8String:imagePath] : nil;
    NSString* text = message ? [NSString stringWithUTF8String:message] : nil;

    NSMutableArray* items = [NSMutableArray array];
    if (text.length > 0)
    {
        [items addObject:text];
    }

    if (path.length > 0)
    {
        UIImage* image = [UIImage imageWithContentsOfFile:path];
        if (image)
        {
            [items addObject:image];
        }
    }

    if (items.count == 0)
    {
        NSLog(@"[IOSShare] No items to share.");
        return;
    }

    dispatch_async(dispatch_get_main_queue(), ^{
        UIViewController* root = UnityGetGLViewController();
        if (root == nil)
        {
            NSLog(@"[IOSShare] Unable to locate root view controller.");
            return;
        }

        UIActivityViewController* activity = [[UIActivityViewController alloc] initWithActivityItems:items applicationActivities:nil];

        if (activity.popoverPresentationController)
        {
            activity.popoverPresentationController.sourceView = root.view;
            activity.popoverPresentationController.sourceRect = CGRectMake(CGRectGetMidX(root.view.bounds), CGRectGetMidY(root.view.bounds), 0, 0);
        }

        [root presentViewController:activity animated:YES completion:nil];
    });
}
