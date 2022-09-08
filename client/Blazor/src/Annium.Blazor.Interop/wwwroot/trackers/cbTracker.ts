const callbacks = new Map<number, Function>()

let id = 0

export type Callback<T> = T & { id: number }

export default {
    track: <T extends Function>(cb: T): Callback<T> => {
        const cid = id++
        callbacks.set(cid, cb)
        const callback = cb as Callback<T>
        callback.id = cid

        return callback
    },
    release: <T extends Function>(id: number): T => {
        const cb = callbacks.get(id)
        if (!cb)
            throw new Error(`Callback ${id} is not tracked`)
        callbacks.delete(id)

        return cb as T
    }
}