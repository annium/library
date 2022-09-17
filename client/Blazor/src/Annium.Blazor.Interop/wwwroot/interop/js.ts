export default {
  readFloat: (data: number, offset: number): number => Blazor.platform.readFloatField(data, offset),
  readShort: (data: number, offset: number): number => Blazor.platform.readInt16Field(data, offset),
  readInt: (data: number, offset: number): number => Blazor.platform.readInt32Field(data, offset),
  readObject: <T>(data: number, offset: number): T => Blazor.platform.readObjectField<T>(data, offset),
  readString: (data: number, offset: number): string => Blazor.platform.readStringField(data, offset),
  readStruct: <T>(data: number, offset: number): T => Blazor.platform.readStructField<T>(data, offset),
  readUlong: (data: number, offset: number): number => Blazor.platform.readUint64Field(data, offset),
  writeString: (value: string): number => BINDING.js_string_to_mono_string(value),
  writeObject: <T>(value: T): number => BINDING.js_to_mono_obj(value),
}
