// noinspection JSUnusedGlobalSymbols

import cbTracker from '../trackers/cbTracker.js'
import objectTracker from '../trackers/objectTracker.js'
import js from '../interop/js.js';
import { getLog } from '../log.js';

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

const log = getLog('element')

/* properties */

// style
export const getStyle = (data: number): number => {
  return js.writeString(getEl(data).style.cssText)
}
export const setStyle = (data: number): void => {
  getEl(data).style.cssText = js.readString(data, 4)
}

/* methods */

export const getBoundingClientRect = (id: string) => {
  const { x, y, width, height } = getById(id).getBoundingClientRect()
  return { x, y, width, height }
}

/* events */

export const onKeyboardEvent = (id: string, type: KeyboardEventName, ref: DotNet.DotNetObject, method: string, preventDefault: boolean): number => {
  const callback = cbTracker.track(preventDefault
    ? (e: KeyboardEvent) => {
      e.preventDefault();
      ref.invokeMethod(method, callback.id, [e.key, e.code, e.metaKey, e.ctrlKey, e.altKey, e.shiftKey])
    }
    : (e: KeyboardEvent) => {
      ref.invokeMethod(method, callback.id, [e.key, e.code, e.metaKey, e.ctrlKey, e.altKey, e.shiftKey])
    })
  log.debug(id, 'onKeyboardEvent', type, 'add callback', callback.id)
  getById(id).addEventListener(type, callback)

  return callback.id
}
export const offKeyboardEvent = (id: string, type: KeyboardEventName, cid: number): void => {
  log.debug(id, 'offKeyboardEvent', type, 'release callback', cid)
  getById(id).removeEventListener(type, cbTracker.release(cid))
}

export const onMouseEvent = (id: string, type: MouseEventName, ref: DotNet.DotNetObject, method: string): number => {
  const callback = cbTracker.track((e: MouseEvent) => {
    e.preventDefault();
    ref.invokeMethod(method, callback.id, [e.clientX, e.clientY])
  })
  log.debug(id, 'onMouseEvent', type, 'add callback', callback.id)
  getById(id).addEventListener(type, callback)

  return callback.id
}
export const offMouseEvent = (id: string, type: MouseEventName, cid: number): void => {
  log.debug(id, 'offMouseEvent', type, 'release callback', cid)
  getById(id).removeEventListener(type, cbTracker.release(cid))
}

export const onWheelEvent = (id: string, type: 'wheel', ref: DotNet.DotNetObject, method: string): number => {
  const callback = cbTracker.track((e: WheelEvent) => {
    e.preventDefault();
    ref.invokeMethod(method, callback.id, [e.ctrlKey, e.deltaX, e.deltaY])
  })
  log.debug(id, 'onWheelEvent', 'add callback', callback.id)
  getById(id).addEventListener(type, callback)

  return callback.id
}
export const offWheelEvent = (id: string, type: 'wheel', cid: number): void => {
  log.debug(id, 'offWheelEvent', type, 'release callback', cid)
  getById(id).removeEventListener(type, cbTracker.release(cid))
}

function getEl(data: number): HTMLElement {
  return objectTracker.get<HTMLElement>(js.readString(data, 0))
}

function getById(id: string): HTMLElement {
  return objectTracker.get<HTMLElement>(id)
}
