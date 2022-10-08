import cbTracker from '../trackers/cbTracker.js';
import { getLog } from '../log.js';

const log = getLog('window')

/* properties */

export const innerWidth = (): number => window.innerWidth
export const innerHeight = (): number => window.innerHeight

/* methods */

export const onResizeEvent = (type: 'resize', ref: DotNet.DotNetObject, method: string): number => {
  const callback = cbTracker.track((_: UIEvent) => {
    ref.invokeMethod(method, callback.id, [window.innerWidth, window.innerHeight])
  })
  log.debug('onResizeEvent', 'add callback', callback.id)
  window.addEventListener(type, callback)

  return callback.id
}
export const offResizeEvent = (type: 'resize', cid: number): void => {
  log.debug('offEvent', type, 'release callback', cid)
  window.removeEventListener(type, cbTracker.release(cid))
}
