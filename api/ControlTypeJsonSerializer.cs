﻿using System;
using System.Reflection;
using Newtonsoft.Json;

namespace api
{
public class LightConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Light);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Light lt = (Light)value;
		writer.WriteValue(lt.Brightness);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Light lt = new Light()
		{
			Brightness = (double)reader.Value
		};

		return lt;
	}
}

public class TemperatureConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Temperature);
	}
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Temperature lt = (Temperature)value;
		writer.WriteValue(lt.C);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Temperature lt = new Temperature()
		{
			C = (double)reader.Value
		};

		return lt;
	}
}

}

