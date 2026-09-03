import cbTracker from './trackers/cbTracker.js'
import { getLog } from './log.js'

export type KeyboardEventName =
  | 'keydown'
  | 'keyup'

export type MouseEventName =
  | 'mousedown'
  | 'mouseenter'
  | 'mouseleave'
  | 'mousemove'
  | 'mouseout'
  | 'mouseover'
  | 'mouseup'

const log = getLog('events')

/**
 * Tracks a keyboard-event callback and attaches it to the target, forwarding the event's key/modifier payload to the
 * .NET side; shared by the window and per-element facades (they differ only in target and log scope).
 */
export const onKeyboardEvent = (target: EventTarget, scope: string, type: KeyboardEventName, ref: DotNet.DotNetObject, method: string, preventDefault: boolean): number => {
  const callback = cbTracker.track(preventDefault
    ? (e: KeyboardEvent) => {
      e.preventDefault();
      ref.invokeMethod(method, callback.id, [e.key, e.code, e.metaKey, e.ctrlKey, e.altKey, e.shiftKey])
    }
    : (e: KeyboardEvent) => {
      ref.invokeMethod(method, callback.id, [e.key, e.code, e.metaKey, e.ctrlKey, e.altKey, e.shiftKey])
    })
  log.debug(scope, 'onKeyboardEvent', type, 'add callback', callback.id)
  target.addEventListener(type, callback as unknown as EventListener)

  return callback.id
}

/**
 * Releases the tracked keyboard-event callback and detaches it from the target.
 */
export const offKeyboardEvent = (target: EventTarget, scope: string, type: KeyboardEventName, cid: number): void => {
  log.debug(scope, 'offKeyboardEvent', type, 'release callback', cid)
  target.removeEventListener(type, cbTracker.release(cid) as unknown as EventListener)
}

/**
 * Tracks a mouse-event callback and attaches it to the target, forwarding the event's position/modifier payload to
 * the .NET side; shared by the window and per-element facades.
 */
export const onMouseEvent = (target: EventTarget, scope: string, type: MouseEventName, ref: DotNet.DotNetObject, method: string): number => {
  const callback = cbTracker.track((e: MouseEvent) => {
    e.preventDefault();
    ref.invokeMethod(method, callback.id, [e.clientX, e.clientY, e.metaKey, e.ctrlKey, e.altKey, e.shiftKey])
  })
  log.debug(scope, 'onMouseEvent', type, 'add callback', callback.id)
  target.addEventListener(type, callback as unknown as EventListener)

  return callback.id
}

/**
 * Releases the tracked mouse-event callback and detaches it from the target.
 */
export const offMouseEvent = (target: EventTarget, scope: string, type: MouseEventName, cid: number): void => {
  log.debug(scope, 'offMouseEvent', type, 'release callback', cid)
  target.removeEventListener(type, cbTracker.release(cid) as unknown as EventListener)
}
