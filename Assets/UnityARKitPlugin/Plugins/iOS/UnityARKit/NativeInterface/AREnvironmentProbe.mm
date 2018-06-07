// Unity Technologies Inc (c) 2017
// AREnvironmentProbe.mm
// Main implementation of ARKit plugin native AREnvironmentProbeAnchor

#include "ARKitDefines.h"



typedef struct
{
    void* identifier;
    UnityARMatrix4x4 transform;
    void* cubemapPtr;
    UnityARVector3 extent;
} UnityAREnvironmentProbeAnchorData;


inline void UnityAREnvironmentProbeAnchorDataFromAREnvironmentProbeAnchorPtr(UnityAREnvironmentProbeAnchorData& anchorData, AREnvironmentProbeAnchor* nativeAnchor)
{
    anchorData.identifier = (void*)[nativeAnchor.identifier.UUIDString UTF8String];
    ARKitMatrixToUnityARMatrix4x4(nativeAnchor.transform, &anchorData.transform);
    anchorData.cubemapPtr = (__bridge_retained void*)[nativeAnchor environmentTexture];
    anchorData.extent = UnityARVector3 {
        nativeAnchor.extent.x,
        nativeAnchor.extent.y,
        nativeAnchor.extent.z
    };
}

typedef void (*UNITY_AR_ENVPROBE_ANCHOR_CALLBACK)(UnityAREnvironmentProbeAnchorData anchorData);


@interface UnityAREnvironmentProbeAnchorCallbackWrapper : NSObject <UnityARAnchorEventDispatcher>
{
@public
    UNITY_AR_ENVPROBE_ANCHOR_CALLBACK _anchorAddedCallback;
    UNITY_AR_ENVPROBE_ANCHOR_CALLBACK _anchorUpdatedCallback;
    UNITY_AR_ENVPROBE_ANCHOR_CALLBACK _anchorRemovedCallback;
}
@end

@implementation UnityAREnvironmentProbeAnchorCallbackWrapper

-(void)sendAnchorAddedEvent:(ARAnchor*)anchor
{
    UnityAREnvironmentProbeAnchorData data;
    UnityAREnvironmentProbeAnchorDataFromAREnvironmentProbeAnchorPtr(data, (AREnvironmentProbeAnchor*)anchor);
    _anchorAddedCallback(data);
}

-(void)sendAnchorRemovedEvent:(ARAnchor*)anchor
{
    UnityAREnvironmentProbeAnchorData data;
    UnityAREnvironmentProbeAnchorDataFromAREnvironmentProbeAnchorPtr(data, (AREnvironmentProbeAnchor*)anchor);
    _anchorRemovedCallback(data);
}

-(void)sendAnchorUpdatedEvent:(ARAnchor*)anchor
{
    UnityAREnvironmentProbeAnchorData data;
    UnityAREnvironmentProbeAnchorDataFromAREnvironmentProbeAnchorPtr(data, (AREnvironmentProbeAnchor*)anchor);
    _anchorUpdatedCallback(data);
}

@end

extern "C" UnityAREnvironmentProbeAnchorData SessionAddEnvironmentProbeAnchor(void* nativeSession, UnityAREnvironmentProbeAnchorData anchorData)
{
    UnityAREnvironmentProbeAnchorData returnAnchorData;

    if (UnityIsARKit_2_0_Supported())
    {
        // create a native AREnvironmentProbeAnchor and add it to the session
        // then return the data back to the user that they will
        // need in case they want to remove it
        UnityARSession* session = (__bridge UnityARSession*)nativeSession;
        matrix_float4x4 initMat;
        UnityARMatrix4x4ToARKitMatrix(anchorData.transform, &initMat);
        AREnvironmentProbeAnchor *newAnchor = [[AREnvironmentProbeAnchor alloc] initWithTransform:initMat];
        
        [session->_session addAnchor:newAnchor];
        UnityAREnvironmentProbeAnchorDataFromAREnvironmentProbeAnchorPtr(returnAnchorData, newAnchor);
    }
    
    return returnAnchorData;

 }


extern "C" void session_SetEnvironmentProbeAnchorCallbacks(const void* session, UNITY_AR_ENVPROBE_ANCHOR_CALLBACK envProbeAnchorAddedCallback,
                                                UNITY_AR_ENVPROBE_ANCHOR_CALLBACK envProbeAnchorUpdatedCallback,
                                                UNITY_AR_ENVPROBE_ANCHOR_CALLBACK envProbeAnchorRemovedCallback)
{
    if (UnityIsARKit_2_0_Supported())
    {
        UnityARSession* nativeSession = (__bridge UnityARSession*)session;
        UnityAREnvironmentProbeAnchorCallbackWrapper* envProbeAnchorCallbacks = [[UnityAREnvironmentProbeAnchorCallbackWrapper alloc] init];
        envProbeAnchorCallbacks->_anchorAddedCallback = envProbeAnchorAddedCallback;
        envProbeAnchorCallbacks->_anchorUpdatedCallback = envProbeAnchorUpdatedCallback;
        envProbeAnchorCallbacks->_anchorRemovedCallback = envProbeAnchorRemovedCallback;
        [nativeSession->_classToCallbackMap setObject:envProbeAnchorCallbacks forKey:[AREnvironmentProbeAnchor class]];
    }
}


