import cbTracker from '../trackers/cbTracker.js';
import {getLog} from '../log.js';

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

const log = getLog('window')

/* properties */

export const innerWidth = (): number => window.innerWidth
export const innerHeight = (): number => window.innerHeight

/* methods */
export const onKeyboardEvent = (type: KeyboardEventName, ref: DotNet.DotNetObject, method: string, preventDefault: boolean): number => {
  const callback = cbTracker.track(preventDefault
    ? (e: KeyboardEvent) => {
      e.preventDefault();
      ref.invokeMethod(method, callback.id, [e.key, e.code, e.metaKey, e.ctrlKey, e.altKey, e.shiftKey])
    }
    : (e: KeyboardEvent) => {
      ref.invokeMethod(method, callback.id, [e.key, e.code, e.metaKey, e.ctrlKey, e.altKey, e.shiftKey])
    })
  log.debug('window', 'onKeyboardEvent', type, 'add callback', callback.id)
  window.addEventListener(type, callback)

  return callback.id
}
export const offKeyboardEvent = (type: KeyboardEventName, cid: number): void => {
  log.debug('window', 'offKeyboardEvent', type, 'release callback', cid)
  window.removeEventListener(type, cbTracker.release(cid))
}

export const onMouseEvent = (type: MouseEventName, ref: DotNet.DotNetObject, method: string): number => {
  const callback = cbTracker.track((e: MouseEvent) => {
    e.preventDefault();
    ref.invokeMethod(method, callback.id, [e.clientX, e.clientY, e.metaKey, e.ctrlKey, e.altKey, e.shiftKey])
  })
  log.debug('window', 'onMouseEvent', type, 'add callback', callback.id)
  window.addEventListener(type, callback)

  return callback.id
}
export const offMouseEvent = (type: MouseEventName, cid: number): void => {
  log.debug('window', 'offMouseEvent', type, 'release callback', cid)
  window.removeEventListener(type, cbTracker.release(cid))
}

export const onResizeEvent = (type: 'resize', ref: DotNet.DotNetObject, method: string): number => {
  const callback = cbTracker.track((_: UIEvent) => {
    ref.invokeMethod(method, callback.id, [window.innerWidth, window.innerHeight])
  })
  log.debug('window', 'onResizeEvent', 'add callback', callback.id)
  window.addEventListener(type, callback)

  return callback.id
}
export const offResizeEvent = (type: 'resize', cid: number): void => {
  log.debug('window', 'offEvent', type, 'release callback', cid)
  window.removeEventListener(type, cbTracker.release(cid))
}
