import { getLog } from '../log.js';

const callbacks = new Map<number, Function>()

let counter = 0

export type Callback<T> = T & { id: number }

const log = getLog('cbTracker')

const track = <T extends Function>(cb: T): Callback<T> => {
  const cid = counter++
  log.trace('track', cid)
  callbacks.set(cid, cb)
  const callback = cb as Callback<T>
  callback.id = cid

  return callback
}

const release = <T extends Function>(cid: number): T => {
  log.trace('release', cid)
  const cb = callbacks.get(cid)
  if (!cb)
    throw new Error(`Callback ${cid} is not tracked`)
  callbacks.delete(cid)

  return cb as T
}

const dump = () => new Map(callbacks)

export default {
  track,
  release,
  dump,
}
