﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Ardoq.Models.Converters
{
	public class ModelConverter : CustomCreationConverter<Model>
	{
		public override object ReadJson (JsonReader reader, Type objectType, object existingValue,
		                                 JsonSerializer serializer)
		{
			JObject jsonObject = JObject.Load (reader);

			var result = (Model)base.ReadJson (jsonObject.CreateReader (), objectType, existingValue, serializer);
			Console.WriteLine ("Parsing model: " + jsonObject.Property ("name"));
			JProperty referenceTypesProperty = jsonObject.Property ("referenceTypes");
			if (referenceTypesProperty != null) {
				var referenceTypesDictionary = referenceTypesProperty.Value.ToObject<Dictionary<int, JObject>> ();
				result.ReferenceTypes = GetReferenceTypes (referenceTypesDictionary);
			}

			JProperty rootProperty = jsonObject.Property ("root");
			if (rootProperty != null) {
				var componentTypesDictionary = rootProperty.Value.ToObject<Dictionary<string, JObject>> ();
				result.ComponentTypes = GetComponentTypes (componentTypesDictionary);
			}

			return result;
		}

		private Dictionary<string, string> GetComponentTypes (Dictionary<string, JObject> componentTypesDictionary)
		{
			var result = new Dictionary<string, string> ();
			foreach (var componentTypePair in componentTypesDictionary) {
				JObject jObject = componentTypePair.Value;
				JProperty nameProperty = jObject.Property ("name");
				JProperty childrenProperty = jObject.Property ("children");
				if (!result.ContainsKey (nameProperty.Value.ToObject<String> ())) {
					result.Add (nameProperty.Value.ToObject<String> (), componentTypePair.Key);
				} else {
					Console.WriteLine ("Cannot properly parse model - Duplicate key: " + nameProperty.Value.ToObject<String> ());
				}
				Dictionary<string, string> subComponentTypes =
					GetComponentTypes (childrenProperty.Value.ToObject<Dictionary<string, JObject>> ());
				foreach (var subComponentType in subComponentTypes) {
					if (!result.ContainsKey (subComponentType.Key)) {
						result.Add (subComponentType.Key, subComponentType.Value);
					} else {
						Console.WriteLine ("Cannot properly parse model  - Duplicate key: " + subComponentType.Key);
					}
				}
			}
			return result;
		}

		private Dictionary<string, int> GetReferenceTypes (Dictionary<int, JObject> referenceTypesDictionary)
		{
			var result = new Dictionary<string, int> ();
			if (null != referenceTypesDictionary) {
				foreach (var referenceTypePair in referenceTypesDictionary) {
					JObject jObject = referenceTypePair.Value;
					JProperty nameProperty = jObject.Property ("name");
					JProperty idProperty = jObject.Property ("id");
					var key = nameProperty.Value.ToObject<string> ();
					var value = idProperty.Value.ToObject<int> ();
					if (result.ContainsKey (key)) {
						// We may end up having the same name for multiple resources, we need t
						var resourceId = jObject.Property ("id").Value.ToObject<string> ();
						key += resourceId;
					}
					result.Add (key, value);
				}
			}
			return result;
		}

		public override Model Create (Type objectType)
		{
			return new Model ();
		}
	}
}