// noinspection JSUnusedGlobalSymbols

import cbTracker, { Callback } from '../trackers/cbTracker.js'
import objectTracker from '../trackers/objectTracker.js'
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
export const getStyle = (id: string): string => {
  return getById(id).style.cssText
}
export const setStyle = (id: string, style: string): void => {
  getById(id).style.cssText = style
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

interface ResizeHandler {
  observer: ResizeObserver;
  callbacks: Set<Callback<(width: number, height: number) => void>>;
}

const resizeHandlers = new Map<string, ResizeHandler>()

export const onResizeEvent = (id: string, _type: 'resize', ref: DotNet.DotNetObject, method: string): number => {
  const callback = cbTracker.track((width: number, height: number) => {
    ref.invokeMethod(method, callback.id, [width, height])
  })
  log.debug(id, 'onResizeEvent', 'add callback', callback.id)
  const handler = resizeHandlers.get(id);
  if (handler) {
    handler.callbacks.add(callback)
  } else {
    const callbacks = new Set<Callback<(width: number, height: number) => void>>();
    callbacks.add(callback)
    const observer = new ResizeObserver(entries => {
      for (const entry of entries) {
        if (!entry.contentBoxSize) continue
        // Firefox implements `contentBoxSize` as a single content rect, rather than an array
        const contentBoxSize = Array.isArray(entry.contentBoxSize) ? entry.contentBoxSize[0] : entry.contentBoxSize;
        for (const callback of callbacks)
          callback(Math.round(contentBoxSize.inlineSize), Math.round(contentBoxSize.blockSize))
      }
    });
    observer.observe(getById(id))
    resizeHandlers.set(id, { observer, callbacks })
  }

  return callback.id
}
export const offResizeEvent = (id: string, _type: 'resize', cid: number): void => {
  log.debug(id, 'offResizeEvent', 'release callback', cid)
  const callback = cbTracker.release<(width: number, height: number) => void>(cid)
  const handler = resizeHandlers.get(id);
  if (!handler)
    return

  handler.callbacks.delete(callback)
  if (handler.callbacks.size)
    return;

  handler.observer.disconnect()
  resizeHandlers.delete(id)
}

function getById(id: string): HTMLElement {
  return objectTracker.get<HTMLElement>(id)
}
