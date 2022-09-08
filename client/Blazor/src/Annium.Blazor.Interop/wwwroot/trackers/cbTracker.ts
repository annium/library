const callbacks = new Map<number, Function>()
// @ts-ignore
window['callbacks'] = callbacks;
let id = 0

export default {
    track: (cb: Function): number => {
        const cid = id++
        callbacks.set(cid, cb)

        return cid
    },
    release: <T extends Function>(id: number): T => {
        const cb = callbacks.get(id)
        if (!cb)
            throw new Error(`Callback ${id} is not tracked`)
        callbacks.delete(id)

        return cb as T
    }
}