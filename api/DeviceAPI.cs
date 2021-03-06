﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using api;
using Hats.Time;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Text;

namespace api
{

public class Interfaces
{
	protected HttpClient _http;
	protected Uri _server;
	protected TimeFrame _frame;

	public Interfaces(string serverAddress, TimeFrame frame = default(TimeFrame))
	{
		_http = new HttpClient();
		_server = new Uri(serverAddress);

		if(frame == null)
		{
			frame = new TimeFrame();
		}

		_frame = frame;
	}

	public Interfaces(Uri serverAddress, TimeFrame frame = default(TimeFrame))
	{
		_server = serverAddress;

		if(frame == null)
		{
			frame = new TimeFrame();
		}

		_frame = frame;
	}


	/**
	 * Function which is called to request a list of devices present at a given location, which
	 * are not currently registered in the HATS system. Since these devices are not registered,
	 * no ID has been assigned.
	 *
	 * \param[in] address Location of the house to query for unregistered devices.
	 * \param[out] List of strings which represent devices which could be registered.
	 */
	public List<string> enumerateDevices(UInt64 house_id)
	{
		string houseID = house_id.ToString();
		var client = new HttpClient();
		client.Timeout = TimeSpan.FromSeconds(50);
		client.BaseAddress = _server;

		var response = client.GetAsync("api/app/device/enumeratedevices/" + houseID).Result;

		List<string> listOfDevices= new List<string>();

		if(!response.IsSuccessStatusCode)
		{
			return listOfDevices;
		}

		try
		{
			var content = response.Content.ReadAsStringAsync();
			content.Wait();
			JArray unregisteredDevices = JArray.Parse(content.Result);
			foreach(JToken Device in unregisteredDevices)
			{
				listOfDevices.Add(Device.ToString());
			}
		}
		catch(JsonException ex)
		{
			Debug.WriteLine("Error reading the list of devices" + ex.Message);
			Debug.WriteLine("Error reading the list of devices" + ex.InnerException.Message);
		}
		return listOfDevices;
	}

	/**
	 * Registers a device with the server, in essence creating it for use in HATS.
	 * \param[in] name User friendly name to use for the device.
	 * \param[in] house_id ID of the house this device is in.
	 * \param[in] info Blob from enumerateDevices() which represents the device of interest
	 * \param[in] room_id ID of the room this device is in, if set.
	 * \param[out] Object providing control of the registered device, or null if it fails to register.
	 */
	public Device registerDevice(string name, UInt64 house_id, string info, UInt64 room_id = 0)
	{
		//TODO: Verify parameters here are sufficient
		if(String.IsNullOrEmpty(info) || String.IsNullOrEmpty(name))
		{
			return null;
		}
		//TODO: Post to Server API to request the device be recorded, and get the device.
		string houseID = house_id.ToString();
		var client = new HttpClient();
		client.Timeout = TimeSpan.FromSeconds(50);
		client.BaseAddress = _server;

		string type = "";
		try
		{
			var obj = JObject.Parse(info);
			obj.GetValue("Class").ToObject<string>();
		}
		catch(JsonException ex)
		{
			Debug.WriteLine("Error registering the device" + ex.Message);
			Debug.WriteLine("Error registering the device" + ex.InnerException.Message);
		}
		var device_register = new
		{
			houseID = house_id,
			roomID = room_id,
			Type = type
		};
		var response = client.PostAsync("api/storage/device/", new StringContent(JsonConvert.SerializeObject(device_register), Encoding.UTF8, "application/json"));
		response.Wait();

		Device output = null;
		if(response.Result.IsSuccessStatusCode)
		{
			var data = response.Result.Content.ReadAsStringAsync();
			data.Wait();
			output = CreateDevice(data.Result, _frame);
		}
		return output;
	}

	/**
	 * Given a device, remove it from the system as a registered device. This should only be done
	 * as a result of a user request, as this deletes all references to the device.
	 * \param[in] Device to remove
	 * \param[out] Flag indicating success
	 */
	public bool deleteDevice(Device dev)
	{
		return true;
	}

	/**
	 * Function to get a list of devices from the server.
	 * \param[in] ID of the House to get devices for
	 * \param[out] List of devices in a given house.
	 */
	public List<Device> getDevices(UInt64 houseID)
	{
		var devices = new List<Device>();
		string house_id = houseID.ToString();
		var client = new HttpClient();
		client.Timeout = TimeSpan.FromSeconds(50);
		client.BaseAddress = _server;

		var response = client.GetAsync("api/storage/device/{houseid}" + houseID).Result;

		if(!response.IsSuccessStatusCode)
		{
			return devices;
		}
		//TODO: Query all devices in a given house.
		try
		{
			var content = response.Content.ReadAsStringAsync();
			content.Wait();
			JArray unregisteredDevices = JArray.Parse(content.Result);
			foreach(JToken Device in unregisteredDevices)
			{
				devices.Add(CreateDevice(Device.ToString(),_frame));
			}
		}
		catch(JsonException ex)
		{
			Debug.WriteLine("Error reading the list of devices" + ex.Message);
			Debug.WriteLine("Error reading the list of devices" + ex.InnerException.Message);
		}
		return devices;
	}

	/**
	 * Function to get a List of devices from the server, given house & room.
	 * \param[in] houseID ID of House to get devices from
	 * \param[in] roomID ID of Room to get devices from. 0 means "Devices not assigned a room"
	 * \param[out] List of Devices matching the requested parameters.
	 */
	public List<Device> getDevices(UInt64 houseID, UInt64 roomID)
	{
		var devices = new List<Device>();
		string house_id = houseID.ToString();
		var client = new HttpClient();
		client.Timeout = TimeSpan.FromSeconds(50);
		client.BaseAddress = _server;

		var response = client.GetAsync("api/storage/device/{houseid}/{spaceid}" + houseID + roomID).Result;

		if(!response.IsSuccessStatusCode)
		{
			return devices;
		}
		//TODO: Query all devices in a given house.
		try
		{
			var content = response.Content.ReadAsStringAsync();
			content.Wait();
			JArray unregisteredDevices = JArray.Parse(content.Result);
			foreach(JToken tok in unregisteredDevices)
			{
				var device = CreateDevice(tok.ToString(),_frame);
				devices.Add(device);
			}
		}
		catch(JsonException ex)
		{
			Debug.WriteLine("Error reading the list of devices" + ex.Message);
			Debug.WriteLine("Error reading the list of devices" + ex.InnerException.Message);
		}
		return devices;
	}

	/**
	 * Given relevant information string, attempts to create a device capable of communicating with a device through the server.
	 * \param[in] info JSON string representing the device, which came from the server
	 * \param[in] frame TimeFrame for timestamping data for this device
	 */
	public Device CreateDevice(string info, TimeFrame frame)
	{
		var inp = new ServerInput(_server.ToString());
		var outp = new ServerOutput(_server.ToString());
		return Interfaces.DeserializeDevice(info, inp, outp, frame);
	}

	/**
	 * Given a JSON string representing a device, instantiates the device as desired. Raw instantiation, which shouldn't
	 * be used by end-clients typically
	 * \param[in] info JSON string representing device. Must have a key named "class" which
	 *            names the class deriving from Device to instantiate.
	 * \param[in] inp IDeviceInput to create device with
	 * \param[in] outp IDeviceOutput to create device with
	 * \param[in] frame TimeFrame to initialize the frame
	 */
	public static Device DeserializeDevice(string info, IDeviceInput inp, IDeviceOutput outp, TimeFrame frame)
	{
		if(String.IsNullOrEmpty(info))
		{
			return null;
		}

		Device device = null;
		try
		{
			JObject device_obj = JObject.Parse(info);
			JToken type_tok;
			if(!device_obj.TryGetValue("class", StringComparison.OrdinalIgnoreCase, out type_tok))
			{
				return null;
			}

			var device_type = GetDeviceType(type_tok.ToString());
			if(device_type != null)
			{
				device = (Device)Activator.CreateInstance(device_type, null, null, frame);
				update(device, info, update_id:true, force:true);
				device.update();
				device.resetIO(inp, outp); //this way, population doesn't trigger house comms
			}
		}
		catch(JsonException ex)
		{
			//TODO: Figure out how to pass exceptions up, or record error
			Debug.WriteLine("Error deserializing the list of devices" + ex.Message);
			Debug.WriteLine("Error deserializing the list of devices" + ex.InnerException.Message);
		}
		return device;
	}

	/**
	 * Attempts to get the type of a specific device, given the fully
	 * qualified name.
	 * \param[in] typeName Fully qualified classname of device
	 * \param[out] Type representing device, or NULL if it doesn't exist
	 */
	private static Type GetDeviceType(string typeName)
	{
		return Type.GetType("api." + typeName);
	}

	/**
	 * Given a device and the JSON string to update it with, this will update
	 * all public properties which are not the TimeFrame to
	 * whatever value is in the JSON blob.
	 * \param[in] dev Device to be updated
	 * \param[in] json JSON blob of fields to update
	 * \param[in] silence_io Temporarily disable the Device IO for simple value updating
	 * \param[in] update_id Flag indicating if the DeviceID should be updated at all
	 * \param[in] force Flag indicating if public/private flags should be respected for the update
	 * \param[out] Flag indicating if at least one field was updated
	 */
	public static bool UpdateDevice(Device dev, string json, bool silence_io = false,
		bool update_id = false)
	{
		return update(dev, json, silence_io, update_id, force: true);
	}

	/**
	 * Given an instance of the device, attempts to update all public properties with info
	 * from the new device instance. Note that devices currently must have the same ID, but
	 * the actual classes do not need to be the same.
	 * \param[in] old_dev Device which will be updated.
	 * \param[in] new_dev Device which contains values to be updated with
	 * \param[in] silence_io Temporarily disable the Device IO for simple value updating
	 * \param[out] Flag indicating if a field was updated with a new value
	 */
	public static bool UpdateDevice(Device old_dev, Device new_dev, bool silence_io = false)
	{
		if(old_dev == null || new_dev == null)
		{
			return false;
		}

		var props = old_dev.GetType().GetRuntimeProperties();
		bool update_field = false;

		IDeviceInput inp = null;
		IDeviceOutput outp = null;

		if(silence_io)
		{
			inp = old_dev.Input;
			outp = old_dev.Output;
			old_dev.resetIO();
		}
		foreach(var info in props)
		{
			//Can't update this method
			if(info.SetMethod == null || !info.SetMethod.IsPublic || info.GetMethod == null)
			{
				continue;
			}

			if(info.Name != "LastUpdate")
			{
				var updated = !(info.GetValue(old_dev).Equals(info.GetValue(new_dev)));
				update_field |= updated;
			}
			info.SetValue(old_dev, info.GetValue(new_dev));
		}

		if(silence_io)
		{
			old_dev.resetIO(inp, outp);
		}
		return update_field;
	}

	/**
	 * Internal device updating function.
	 * \param[in] dev Device to be updated
	 * \param[in] json JSON blob of fields to update
	 * \param[in] silence_io Temporarily disable the Device IO for simple value updating
	 * \param[in] update_id Flag indicating if the DeviceID should be updated at all
	 * \param[in] force Flag indicating if public/private flags should be respected for the update
	 * \param[out] Flag indicating if at least one field was updated
	 */
	protected static bool update(Device dev, string json, bool silence_io = false,
		bool update_id = false, bool force = false)
	{
		if(dev == null)
		{
			return false;
		}

		IDeviceInput inp = null;
		IDeviceOutput outp = null;
		if(silence_io)
		{
			inp = dev.Input;
			outp = dev.Output;
			dev.resetIO();
		}
		bool updated_value = true;
		try
		{
			var props = dev.GetType().GetRuntimeProperties();
			var json_obj = JObject.Parse(json);

			foreach(var info in props)
			{
				if(info.SetMethod == null || !(force || info.SetMethod.IsPublic) || info.Name == "Frame")
				{
					continue;
				}
				JToken field;
				if(!json_obj.TryGetValue(info.Name, StringComparison.OrdinalIgnoreCase, out field))
				{
					continue;
				}

				if(update_id && info.Name == "ID")
				{
					if(field.Type == JTokenType.Integer)
					{
						dev.ID.DeviceID = field.ToObject<UInt64>();
					}
					else
					{
						dev.ID = field.ToObject<FullID>();
					}
				}
				else if(info.Name != "ID")
				{
					var value = field.ToObject(info.PropertyType);
					info.SetValue(dev, value);
				}
				updated_value = true;
			}
		}
		catch(JsonException ex)
		{
			//TODO: Report error somehow?
			Debug.WriteLine("Error updating the device" + ex.Message);
			Debug.WriteLine("Error updating the device" + ex.InnerException.Message);
		}

		if(silence_io)
		{
			dev.resetIO(inp, outp);
		}
		return updated_value;
	}
}

}
