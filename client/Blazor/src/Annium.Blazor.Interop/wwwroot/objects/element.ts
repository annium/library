// noinspection JSUnusedGlobalSymbols

import cbTracker from '../trackers/cbTracker.js'
import objectTracker from '../trackers/objectTracker.js'
import js from '../interop/js.js';

type KeyboardEventName =
    | 'keydown'
    | 'keyup'

type MouseEventName =
    | 'mousedown'
    | 'mouseenter'
    | 'mouseleave'
    | 'mousemove'
    | 'mouseout'
    | 'mouseover'
    | 'mouseup'

export default {
    /* properties */

    // style
    getStyle: function (data: number): number {
        return js.writeString(getEl(data).style.cssText)
    },
    setStyle: function (data: number): void {
        getEl(data).style.cssText = js.readString(data, 4)
    },

    /* methods */

    getBoundingClientRect: (id: string) => {
        const { x, y, width, height } = getById(id).getBoundingClientRect()
        return { x, y, width, height }
    },

    /* events */

    onKeyboardEvent: (id: string, type: KeyboardEventName, ref: DotNet.DotNetObject, method: string, preventDefault: boolean): number => {
        let callbackId: number;
        const callback = preventDefault
            ? (e: KeyboardEvent) => {
                e.preventDefault();
                ref.invokeMethod(method, callbackId, e.key, e.code, e.metaKey, e.ctrlKey, e.altKey, e.shiftKey)
            }
            : (e: KeyboardEvent) => {
                ref.invokeMethod(method, callbackId, e.key, e.code, e.metaKey, e.ctrlKey, e.altKey, e.shiftKey)
            }
        getById(id).addEventListener(type, callback)

        return callbackId = cbTracker.track(callback)
    },
    onMouseEvent: (id: string, type: MouseEventName, ref: DotNet.DotNetObject, method: string): number => {
        let callbackId: number;
        const callback = (e: MouseEvent) => {
            e.preventDefault();
            ref.invokeMethod(method, callbackId, e.clientX, e.clientY)
        }
        getById(id).addEventListener(type, callback)

        return callbackId = cbTracker.track(callback)
    },
    onWheelEvent: (id: string, type: 'wheel', ref: DotNet.DotNetObject, method: string): number => {
        let callbackId: number;
        const callback = (e: WheelEvent) => {
            e.preventDefault();
            ref.invokeMethod(method, callbackId, e.ctrlKey, e.deltaX, e.deltaY)
        }
        getById(id).addEventListener(type, callback)

        return callbackId = cbTracker.track(callback)
    },
    offEvent: (id: string, type: keyof HTMLElementEventMap, cid: number): void => {
        getById(id).removeEventListener(type, cbTracker.release(cid))
    },
}

function getEl(data: number): HTMLElement {
    return objectTracker.get<HTMLElement>(js.readString(data, 0))
}

function getById(id: string): HTMLElement {
    return objectTracker.get<HTMLElement>(id)
}
