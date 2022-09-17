import cbTracker from '../trackers/cbTracker.js';
import { getLog } from '../log.js';

const log = getLog('window')

export default {
  innerWidth: (): number => window.innerWidth,
  innerHeight: (): number => window.innerHeight,
  onResizeEvent: (type: 'resize', ref: DotNet.DotNetObject, method: string): number => {
    const callback = cbTracker.track((_: UIEvent) => {
      ref.invokeMethod(method, callback.id, [window.innerWidth, window.innerHeight])
    })
    log.debug('onResizeEvent', 'add callback', callback.id)
    window.addEventListener(type, callback)

    return callback.id
  },
  offEvent: (type: keyof WindowEventMap, cid: number): void => {
    log.debug('offEvent', type, 'release callback', cid)
    window.removeEventListener(type, cbTracker.release(cid))
  },
}