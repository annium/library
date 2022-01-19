const objects = new Map<string, HTMLElement>()

export default {
    track: (id: string, el: HTMLElement): void => {
        if (objects.has(id))
            throw new Error(`Object ${id} is already tracked`)

        el.id = id
        objects.set(id, el)
    },
    get: <T extends HTMLElement>(id: string): T => {
        const object = objects.get(id)
        if (!object)
            throw new Error(`Object ${id} is not tracked`)

        return object as T
    },
    release: (id: string): void => {
        if (!objects.delete(id))
            throw new Error(`Object ${id} is not tracked`)
    }
}