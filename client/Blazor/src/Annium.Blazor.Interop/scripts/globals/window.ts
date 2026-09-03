import cbTracker from '../trackers/cbTracker.js';
import { getLog } from '../log.js';
import * as events from '../events.js';
import { KeyboardEventName, MouseEventName } from '../events.js';

const log = getLog('window')

/* properties */

export const innerWidth = (): number => window.innerWidth
export const innerHeight = (): number => window.innerHeight

/* methods */
export const onKeyboardEvent = (type: KeyboardEventName, ref: DotNet.DotNetObject, method: string, preventDefault: boolean): number =>
  events.onKeyboardEvent(window, 'window', type, ref, method, preventDefault)
export const offKeyboardEvent = (type: KeyboardEventName, cid: number): void =>
  events.offKeyboardEvent(window, 'window', type, cid)

export const onMouseEvent = (type: MouseEventName, ref: DotNet.DotNetObject, method: string): number =>
  events.onMouseEvent(window, 'window', type, ref, method)
export const offMouseEvent = (type: MouseEventName, cid: number): void =>
  events.offMouseEvent(window, 'window', type, cid)

export const onResizeEvent = (type: 'resize', ref: DotNet.DotNetObject, method: string): number => {
  const callback = cbTracker.track((_: UIEvent) => {
    ref.invokeMethod(method, callback.id, [window.innerWidth, window.innerHeight])
  })
  log.debug('window', 'onResizeEvent', 'add callback', callback.id)
  window.addEventListener(type, callback)

  return callback.id
}
export const offResizeEvent = (type: 'resize', cid: number): void => {
  log.debug('window', 'offResizeEvent', type, 'release callback', cid)
  window.removeEventListener(type, cbTracker.release(cid))
}
