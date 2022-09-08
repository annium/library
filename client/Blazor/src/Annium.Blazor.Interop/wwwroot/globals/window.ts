import cbTracker from "../trackers/cbTracker.js";

export default {
    innerWidth: (): number => window.innerWidth,
    innerHeight: (): number => window.innerHeight,
    onResizeEvent: (type: 'resize', ref: DotNet.DotNetObject, method: string): number => {
        const callback = cbTracker.track((_: UIEvent) => {
            ref.invokeMethod(method, callback.id, [window.innerWidth, window.innerHeight])
        })
        window.addEventListener(type, callback)

        return callback.id
    },
    offEvent: (type: keyof WindowEventMap, cid: number): void => {
        window.removeEventListener(type, cbTracker.release(cid))
    },
}