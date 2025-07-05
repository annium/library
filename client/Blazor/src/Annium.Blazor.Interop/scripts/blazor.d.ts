declare namespace Blazor {
  const platform: {
    // beginHeapLock: ƒ ()
    // callEntryPoint: async ƒ (e)
    // getArrayEntryPtr: ƒ (e,t,n)
    // getArrayLength: ƒ (e)
    // getObjectFieldsBaseAddress: ƒ (e)
    // invokeWhenHeapUnlocked: ƒ (e)
    readFloatField: (address: number, offset: number) => number
    readInt16Field: (address: number, offset: number) => number
    readInt32Field: (address: number, offset: number) => number
    readObjectField: <T>(address: number, offset: number) => T
    readStringField: (address: number, offset: number) => string
    readStructField: <T>(address: number, offset: number) => T
    readUint64Field: (address: number, offset: number) => number
    // start: ƒ(t)
    // toUint8Array: ƒ(e)
  }
}

declare namespace BINDING {
  function js_string_to_mono_string(value: string): number

  function js_to_mono_enum(obj: number): number

  function js_to_mono_obj(object: unknown): number
}

declare namespace DotNet {
  /**
   * Invokes the specified .NET public method synchronously. Not all hosting scenarios support
   * synchronous invocation, so if possible use invokeMethodAsync instead.
   *
   * @param assemblyName The short name (without key/version or .dll extension) of the .NET assembly containing the method.
   * @param methodIdentifier The identifier of the method to invoke. The method must have a [JSInvokable] attribute specifying this identifier.
   * @param args Arguments to pass to the method, each of which must be JSON-serializable.
   * @returns The result of the operation.
   */
  function invokeMethod<T>(assemblyName: string, methodIdentifier: string, ...args: any[]): T

  /**
   * Invokes the specified .NET public method asynchronously.
   *
   * @param assemblyName The short name (without key/version or .dll extension) of the .NET assembly containing the method.
   * @param methodIdentifier The identifier of the method to invoke. The method must have a [JSInvokable] attribute specifying this identifier.
   * @param args Arguments to pass to the method, each of which must be JSON-serializable.
   * @returns A promise representing the result of the operation.
   */
  function invokeMethodAsync<T>(assemblyName: string, methodIdentifier: string, ...args: any[]): Promise<T>

  /**
   * Represents the .NET instance passed by reference to JavaScript.
   */
  interface DotNetObject {
    /**
     * Invokes the specified .NET instance public method synchronously. Not all hosting scenarios support
     * synchronous invocation, so if possible use invokeMethodAsync instead.
     *
     * @param methodIdentifier The identifier of the method to invoke. The method must have a [JSInvokable] attribute specifying this identifier.
     * @param args Arguments to pass to the method, each of which must be JSON-serializable.
     * @returns The result of the operation.
     */
    invokeMethod<T>(methodIdentifier: string, ...args: any[]): T

    /**
     * Invokes the specified .NET instance public method asynchronously.
     *
     * @param methodIdentifier The identifier of the method to invoke. The method must have a [JSInvokable] attribute specifying this identifier.
     * @param args Arguments to pass to the method, each of which must be JSON-serializable.
     * @returns A promise representing the result of the operation.
     */
    invokeMethodAsync<T>(methodIdentifier: string, ...args: any[]): Promise<T>
  }
}