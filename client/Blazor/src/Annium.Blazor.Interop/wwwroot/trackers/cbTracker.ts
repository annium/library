import { getLog } from '../log.js';

const callbacks = new Map<number, Function>()

let id = 0

export type Callback<T> = T & { id: number }

const log = getLog('cbTracker')

export default {
  track: <T extends Function>(cb: T): Callback<T> => {
    const cid = id++
    log.trace('track', cid)
    callbacks.set(cid, cb)
    const callback = cb as Callback<T>
    callback.id = cid

    return callback
  },
  release: <T extends Function>(cid: number): T => {
    log.trace('release', cid)
    const cb = callbacks.get(cid)
    if (!cb)
      throw new Error(`Callback ${cid} is not tracked`)
    callbacks.delete(cid)

    return cb as T
  }
}