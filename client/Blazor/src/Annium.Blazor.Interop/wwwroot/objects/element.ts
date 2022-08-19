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

    // keyboard event
    onKeyboardEvent: (id: string, type: KeyboardEventName, ref: DotNet.DotNetObject, method: string): number => {
        const callback = (e: KeyboardEvent) => {
            e.preventDefault();
            ref.invokeMethod(method, type, e.key, e.code, e.metaKey, e.shiftKey, e.altKey)
        }
        getById(id).addEventListener(type, callback)

        return cbTracker.track(callback)
    },
    offKeyboardEvent: (id: string, type: KeyboardEventName, cid: number): void => {
        getById(id).removeEventListener(type, cbTracker.release(cid))
    },

    // mouse event
    onMouseEvent: (id: string, type: MouseEventName, ref: DotNet.DotNetObject, method: string): number => {
        const callback = (e: MouseEvent) => {
            e.preventDefault();
            ref.invokeMethod(method, type, e.clientX, e.clientY)
        }
        getById(id).addEventListener(type, callback)

        return cbTracker.track(callback)
    },
    offMouseEvent: (id: string, type: MouseEventName, cid: number): void => {
        getById(id).removeEventListener(type, cbTracker.release(cid))
    },

    // wheel event
    onWheelEvent: (id: string, ref: DotNet.DotNetObject, method: string): number => {
        const callback = (e: WheelEvent) => {
            e.preventDefault();
            ref.invokeMethod(method, e.ctrlKey, e.deltaX, e.deltaY)
        }
        getById(id).addEventListener('wheel', callback)

        return cbTracker.track(callback)
    },
    offWheelEvent: (id: string, cid: number): void => {
        getById(id).removeEventListener('wheel', cbTracker.release(cid))
    },
}

function getEl(data: number): HTMLElement {
    return objectTracker.get<HTMLElement>(js.readString(data, 0))
}

function getById(id: string): HTMLElement {
    return objectTracker.get<HTMLElement>(id)
}
