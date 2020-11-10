#if UNITY_EDITOR && UNITY_ANDROID

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace HmsPlugin
{
	[Serializable]
	public struct ClientConfig
	{
		public string cp_id;
		public string product_id;
		public string client_id;
		public string client_secret;
		public string app_id;
		public string package_name;
		public string api_key;
	}
	[Serializable]
	public struct AppGalleryConfig
	{
		public ClientConfig client;
		public string configuration_version;
	}

	public class HuaweiEditor : EditorWindow
	{
		private const float LabelWidth = 100f;

		private const string AgConnectAgCp = "com.huawei.agconnect:agcp:1.4.1.300";
		private const string AgConnectAuth = "com.huawei.agconnect:agconnect-auth:1.4.1.300";
		private const string AdsLite = "com.huawei.hms:ads-lite:13.4.34.300";
		private const string AdsConsent = "com.huawei.hms:ads-consent:3.4.34.300";
		private const string Analytics = "com.huawei.hms:hianalytics:5.0.4.301";
		private const string AgConnect = "com.huawei.agconnect:agconnect-core:1.0.0.300";
		private const string Base = "com.huawei.hms:base:5.0.4.301";
		private const string HwId = "com.huawei.hms:hwid:5.0.3.301";
		private const string Iap = "com.huawei.hms:iap:5.0.2.300";
		private const string Crash = "com.huawei.agconnect:agconnect-crash:1.4.1.300";
		private const string NetworkCommon = "com.huawei.hms:network-common:4.0.0.302";
		private const string NetworkGrs = "com.huawei.hms:network-grs:4.0.0.302";
		private const string OpenDevice = "com.huawei.hms:opendevice:5.0.2.300";
		private const string Push = "com.huawei.hms:push:5.0.2.300";
		private const string Tasks = "com.huawei.hmf:tasks:1.3.3.300";

		private static readonly string[] m_dependencies = {
			AdsLite, AdsConsent, AgConnectAgCp, AgConnectAuth, Analytics, AgConnect, Base, HwId, Iap, NetworkCommon, NetworkGrs, OpenDevice, Push, Tasks, Crash
		};

		private const string RegexPackage = "^([A-Za-z]{1}[A-Za-z\\d_]*\\.)+[A-Za-z][A-Za-z\\d_]*$";
		private const string regexIDs = "^([0.9\\d_])";
		private const string AgConnectServicesJsonFileName = "agconnect-services.json";

		private static readonly string dependenciesXmlTemplate =
			@"<dependencies>
				<androidPackages>
					{0}
				</androidPackages>
			</dependencies>";
		private static readonly string dependenciesTemplate =
			@"<androidPackage spec=""{0}"">
				<repositories>
					<repository>https://developer.huawei.com/repo/</repository>
				</repositories>
			</androidPackage>";

		//#if UNITY_EDITOR
		private bool m_appPackageEntered;
		private bool m_appIdEntered;
		private bool m_cpIdEntered;

		private static string m_appid = "1234567890";
		private static string m_package = "com.yourcompany.yourgame";
		private static string m_cpid = "1234567890";
		
		private GUIStyle m_redGuiStyle;

		private static string configPath => Application.dataPath + "/" + AgConnectServicesJsonFileName;

		#if HUAWEI_APPGALLERY
		[MenuItem("Build/Configure Manifest For AppGallery")]
		#endif
		private static void Init()
		{
			var window = (HuaweiEditor) GetWindow((typeof(HuaweiEditor)));
			window.Show();
		}

		private void OnEnable()
		{
			TryLoadConfig(configPath);
		}

		public static void TryConfigureManifest(bool isHuawei = true)
		{
			if (TryLoadConfig(configPath))
			{
				ConfigureManifest(isHuawei);
			}
		}

		private void OnGUI()
		{
			//Styles
			m_redGuiStyle = new GUIStyle();
			m_redGuiStyle.normal.textColor = Color.red;
			m_redGuiStyle.padding = new RectOffset(10, 0, 0, 0);

			if (GUILayout.Button($"Load {AgConnectServicesJsonFileName}"))
			{
				EditorApplication.update += LoadAgConnectServicesJson;
			}

			var guiContentPackage = new GUIContent("Package", "your bundle identifier");

			//Application Package
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label(guiContentPackage, GUILayout.MinWidth(LabelWidth));
			m_package = EditorGUILayout.TextField(m_package);
			GUILayout.FlexibleSpace();
			m_appPackageEntered = EditorGUILayout.Toggle(m_appPackageEntered);
			m_appPackageEntered = Regex.IsMatch(m_package, RegexPackage) && !m_package.Equals("com.yourcompany.yourgame");
			EditorGUILayout.EndHorizontal();

			//APP ID 
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("App ID", GUILayout.MinWidth(LabelWidth));
			m_appid = EditorGUILayout.TextField(m_appid);
			GUILayout.FlexibleSpace();
			m_appIdEntered = EditorGUILayout.Toggle(m_appIdEntered);
			m_appIdEntered = Regex.IsMatch(m_appid, regexIDs) && !m_appid.Equals("1234567890");
			EditorGUILayout.EndHorizontal();

			//CPID 
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("CPID", GUILayout.MinWidth(LabelWidth));
			m_cpid = EditorGUILayout.TextField(m_cpid);
			GUILayout.FlexibleSpace();
			m_cpIdEntered = EditorGUILayout.Toggle(m_cpIdEntered);
			m_cpIdEntered = Regex.IsMatch(m_cpid, regexIDs) && !m_cpid.Equals("1234567890");
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10f);

			if (m_appPackageEntered && m_appIdEntered && m_cpIdEntered)
			{
				if (GUILayout.Button("Configure Manifest"))
				{
					ConfigureManifest(true);
				}
			}
			else
			{
				if (!m_appPackageEntered)
				{
					GUILayout.Label("Please enter a valid package name", m_redGuiStyle);
				}

				if (!m_appIdEntered)
				{
					GUILayout.Label("Please enter a valid app ID", m_redGuiStyle);
				}

				if (!m_cpIdEntered)
				{
					GUILayout.Label("Please enter a valid CPI", m_redGuiStyle);
				}
			}
		}

		private static void ConfigureManifest(bool isHuawei)
		{
			if (isHuawei)
			{
				PrepareManifest();
			}
			PrepareProjectSettings(isHuawei);
			UpdateDependencies(isHuawei);
		}

		private static void PrepareProjectSettings(bool isHuawei)
		{
			PlayerSettings.applicationIdentifier = isHuawei ? m_package : m_package.Replace(".huawei", "");
		}

		private static void PrepareManifest()
		{
			var androidPluginsPath = Path.Combine(Application.dataPath, "Plugins");
			androidPluginsPath = Path.Combine(androidPluginsPath, "Android");

			var appManifestPath = Path.Combine(Application.dataPath, "Plugins");
			appManifestPath = Path.Combine(appManifestPath, "Android");
			appManifestPath = Path.Combine(appManifestPath, "AndroidManifest.xml");

			var defaultManifestPath = Path.Combine(Directory.GetParent(EditorApplication.applicationPath).FullName, "Data");
#if UNITY_EDITOR_OSX
            defaultManifestPath = Directory.GetParent(EditorApplication.applicationPath).FullName;
#endif
			defaultManifestPath = Path.Combine(defaultManifestPath, "PlaybackEngines");
			defaultManifestPath = Path.Combine(defaultManifestPath, "AndroidPlayer");
			defaultManifestPath = Path.Combine(defaultManifestPath, "Apk");

			var versions = Application.unityVersion.Split('.');
			if (int.TryParse(versions[0], out var result) && result >= 2019 && int.TryParse(versions[1], out result) && result >= 3)
			{
				defaultManifestPath = Path.Combine(defaultManifestPath, "UnityManifest.xml");
			}
			else
			{
				defaultManifestPath = Path.Combine(defaultManifestPath, "AndroidManifest.xml");
			}


			Debug.Log("preparing");

			Debug.Log(defaultManifestPath);

			// Check if user has already created AndroidManifest.xml file in its location.
			// If not, use default Unity AndroidManifest.xml.
			if (!File.Exists(appManifestPath))
			{
				if (!Directory.Exists(androidPluginsPath))
				{
					Directory.CreateDirectory(androidPluginsPath);
				}


				File.Copy(defaultManifestPath, appManifestPath);

				Debug.Log("[HMS]: User defined AndroidManifest.xml file not found in Plugins/Android folder.");
				Debug.Log("[HMS]: Creating default app's AndroidManifest.xml from Unity's default file.");
			}
			else
			{
				// manifestFound = true;
				Debug.Log("[HMS]: AndroidManifest.xml file already exists in Plugins/Android folder.");
			}


			// OPEN MANIFEST FILE
			var manifestFile = new XmlDocument();
			manifestFile.Load(appManifestPath);

			// Add needed permissions if they are missing.
			AddPermissions(manifestFile);

			// Add meta data
			AddMetaData(manifestFile);

			//Add provider
			AddProvider(manifestFile);

			// Save the changes.
			manifestFile.Save(appManifestPath);

			Debug.Log("[HMS]: App's AndroidManifest.xml ready");
		}

		private static void LoadAgConnectServicesJson()
		{
			EditorApplication.update -= LoadAgConnectServicesJson;

			var filePath = EditorUtility.OpenFilePanel($"Open {AgConnectServicesJsonFileName}", Application.dataPath, "json");
			if (!string.IsNullOrEmpty(filePath) && filePath != configPath && TryLoadConfig(filePath))
			{
				File.Copy(filePath, configPath, true);
			}
		}

		private static bool TryLoadConfig(string filePath)
		{
			var config = JsonUtility.FromJson<AppGalleryConfig>(File.ReadAllText(filePath));
			if (config.client.package_name != null)
			{
				m_cpid = config.client.cp_id;
				m_package = config.client.package_name;
				m_appid = config.client.app_id;
				return true;
			}

			return false;
		}

		private static void AddPermissions(XmlDocument manifest)
		{
			var manifestRoot = manifest.DocumentElement;

			var permissions = new[]
			{
				"com.huawei.appmarket.service.commondata.permission.GET_COMMON_DATA",
				"android.permission.REQUEST_INSTALL_PACKAGES",
				"android.permission.INTERNET",
				"android.permission.WRITE_EXTERNAL_STORAGE",
				"android.permission.ACCESS_WIFI_STATE",
				"android.permission.ACCESS_NETWORK_STATE",
				"android.permission.READ_PHONE_STATE"
			};
			
			var permissionsValues = new bool[permissions.Length];
			
			if (manifestRoot != null)
			{
				foreach (XmlNode node in manifestRoot.ChildNodes)
				{
					if (node.Name == "uses-permission" && node.Attributes != null)
					{
						foreach (XmlAttribute attribute in node.Attributes)
						{
							for (var i = 0; i < permissions.Length; i++)
							{
								if (attribute.Value.Contains(permissions[i]))
								{
									permissionsValues[i] = true;
								}
							}
						}
					}
				}

				for (var i = 0; i < permissions.Length; i++)
				{
					WritePermission(permissionsValues[i], permissions[i], manifest);
				}
			}
		}

		private static void WritePermission(bool hasPermission, string permission, XmlDocument manifest)
		{
			if (!hasPermission)
			{
				var element = manifest.CreateElement("uses-permission");
				element.SetAttribute("name", "http://schemas.android.com/apk/res/android", permission);
				manifest.DocumentElement?.AppendChild(element);
				Debug.Log($"[HMS]: {permission} added to manifest.");
			}
			else
			{
				Debug.Log($"[HMS]: Your app's AndroidManifest.xml file already contains {permission}.");
			}
		}

		private static void AddProvider(XmlDocument manifest)
		{
			var providers = new[] {"com.huawei.hms.update.provider.UpdateProvider", "com.huawei.updatesdk.fileprovider.UpdateSdkFileProvider"};
			var authorities = new[] {m_package + ".hms.update.provider", m_package + ".updateSdk.fileProvider"};
			var providerNodes = new XmlNode[providers.Length];

			var manifestRoot = manifest.DocumentElement;
			var applicationNode = manifestRoot?.SelectSingleNode("application");

			if (applicationNode != null)
			{
				foreach (XmlNode node in applicationNode.ChildNodes)
				{
					if (node.Name == "provider" && node.Attributes != null)
					{
						foreach (XmlAttribute attribute in node.Attributes)
						{
							for (var i = 0; i < providers.Length; i++)
							{
								if (attribute.Value.Contains(providers[i]))
								{
									providerNodes[i] = node;
								}
							}
						}
					}
				}

				for (var i = 0; i < providers.Length; i++)
				{
					WriteProvider(manifest, providerNodes[i], providers[i], authorities[i]);
				}
			}
		}

		private static void WriteProvider(XmlDocument manifest, XmlNode node, string provider, string value)
		{
			var applicationNode = manifest.DocumentElement?.SelectSingleNode("application");

			if (node == null)
			{
				var elementSdk = manifest.CreateElement("provider");
				elementSdk.SetAttribute("name", "http://schemas.android.com/apk/res/android", provider);
				elementSdk.SetAttribute("authorities", "http://schemas.android.com/apk/res/android", value);
				elementSdk.SetAttribute("replace", "http://schemas.android.com/tools", "android:authorities");
				elementSdk.SetAttribute("exported", "http://schemas.android.com/apk/res/android", "false");
				elementSdk.SetAttribute("grantUriPermissions", "http://schemas.android.com/apk/res/android", "true");
				applicationNode?.AppendChild(elementSdk);
				Debug.Log($"[HMS]: {provider} added to manifest.");
			}
			else if (node.Attributes != null)
			{
				node.Attributes["authorities", "http://schemas.android.com/apk/res/android"].Value = value;
				Debug.Log($"[HMS]: {provider} edited in manifest.");
			}
		}

		private static void AddMetaData(XmlDocument manifest)
		{
			var metaData = new[] {"com.huawei.hms.client.appid", "com.huawei.hms.client.cpid", "install_channel"};
			var values = new[] {"appid=" + m_appid, "cpid=" + m_cpid, "AppGallery"};
			var nodes = new XmlNode[metaData.Length];
			
			var manifestRoot = manifest.DocumentElement;

			var applicationNode = manifestRoot?.SelectSingleNode("application");
			if (applicationNode != null)
			{
				foreach (XmlNode node in applicationNode.ChildNodes)
				{
					if (node.Name == "meta-data" && node.Attributes != null)
					{
						foreach (XmlAttribute attribute in node.Attributes)
						{
							for (var i = 0; i < metaData.Length; i++)
							{
								if (attribute.Value.Contains(metaData[i]))
								{
									nodes[i] = node;
								}
							}
						}
					}
				}

				for (var i = 0; i < metaData.Length; i++)
				{
					WriteMetadata(manifest, nodes[i], metaData[i], values[i]);
				}
			}
		}

		private static void WriteMetadata(XmlDocument manifest, XmlNode node, string name, string value)
		{
			var applicationNode = manifest.DocumentElement?.SelectSingleNode("application");
			if (node == null)
			{
				var element = manifest.CreateElement("meta-data");
				element.SetAttribute("name", "http://schemas.android.com/apk/res/android", name);
				element.SetAttribute("value", "http://schemas.android.com/apk/res/android", value);
				applicationNode?.AppendChild(element);
				Debug.Log($"[HMS]: {name} added to manifest.");
			}
			else if (node.Attributes != null)
			{
				node.Attributes["value", "http://schemas.android.com/apk/res/android"].Value = value;
				Debug.Log($"[HMS]: {name} updated in manifest.");
			}
		}

		public static void UpdateDependencies(bool isHuawei)
		{
			var filePath = FullPath("Modules/huawei-appgallery-package/Editor/AppDependencies.xml");
			if (isHuawei)
			{
				File.WriteAllText(filePath, GenerateDependencies());
			}
			else
			{
				File.Delete(filePath);
			}
		} 
			
		private static string GenerateDependencies()
		{
			var builder = new StringBuilder();
			foreach (var dependency in m_dependencies)
			{
				builder.Append(string.Format(dependenciesTemplate, dependency));
				builder.Append("\n");
			}
			var dependencies = builder.ToString();

			return string.Format(dependenciesXmlTemplate, dependencies);
		}

		private static string FullPath(string assetsRelatedPath)
		{
			string fullPath = Path.Combine(Application.dataPath, assetsRelatedPath);
			if (Path.DirectorySeparatorChar != '/')
			{
				fullPath = fullPath.Replace('/', Path.DirectorySeparatorChar);
			}

			return fullPath;
		}
	}
}
#endif