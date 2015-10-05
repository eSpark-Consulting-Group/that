using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Valkyrie.ClientTest
{
    public static class MockData
    {
        public static List<APTAT.Valkyrie.Client.ValkyrieFileInfo> GetBadFiles()
        {
            var badFiles = new System.Collections.Generic.List<APTAT.Valkyrie.Client.ValkyrieFileInfo>
            {
	            new APTAT.Valkyrie.Client.ValkyrieFileInfo
	            {
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\bases_unpack.zip",
		Sha1 = "BEE054420528F8A5401F3AA27EBECF9422678450"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\risklevels.zip",
		Sha1 = "4D794D00755668C79DF9C1149040CFDE5D751307"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\bases.zip",
		Sha1 = "793245617E49CB0852FF1C3F6897E823A8FFF67E"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\dunpack.zip",
		Sha1 = "08E161DADE6C33E25213F6189258C84241C0B8C2"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "c:\\program files (x86)\\postgresql\\9.4\\bin\\postgres.exe",
		Sha1 = "CE9F0BBBB3000F3F07AB06D4E20788B91A69B598"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\Trojan zip and rar.rar",
		Sha1 = "727C75F30FA2869062A24C7173DCF2EA1710D677"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\Keygen Adobe Acrobat 7.0.exe",
		Sha1 = "EE2DD7A7DB919336DF9D2CCE6A0D529939455889"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "c:\\program files (x86)\\common files\\freemake shared\\productupdater\\productupdater.exe",
		Sha1 = "F4A7262BBA709E63BE58854FE7BDA047A47280E9"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\rar_size.zip",
		Sha1 = "4E418992203352453DCE6DD91BEDB344A16F4C99"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "c:\\users\\user\\appdata\\roaming\\microsoft\\windows\\start menu\\programs\\startup\\monitor.exe",
		Sha1 = "4C1944D7678D44D11EF13A850BBD7F988A6E08A8"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\42_file_bomb.zip",
		Sha1 = "6C42C62696616B72BBFC88A4BE4EAD57AA7BC503"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\viruses_in_folders.zip",
		Sha1 = "476773EFDA9C938B927AB93A28A23D9609BCFFF0"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\File_ID.Diz",
		Sha1 = "D7B710E649706D16491EBAE75DF81C96A34B5D9D"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\packs.zip",
		Sha1 = "BDDC5FB04D17E1C6FC1F4A6D401A043EF35CD03B"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "c:\\program files (x86)\\postgresql\\9.4\\bin\\pg_ctl.exe",
		Sha1 = "DDA9443FCAC61A8B98CC42C197BFCADBD58D71FD"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\ZXXG.zip",
		Sha1 = "B5205A93056A3B0B124AA26A2187763A8237EE7E"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\1Gb_zero_exe.zip",
		Sha1 = "84A35C94D0BD20D57121082CA41A219C5546E640"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\rar_size_MAC.zip",
		Sha1 = "7E6D38C151F554072626555DC5CBEFE23575931D"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\ADS files.rar",
		Sha1 = "5F387CE458F90001E64E4208074BBA0A2DA401CA"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\jar.zip",
		Sha1 = "1D3F60A357BDF8D16BD9D718B5B63D2DDBB89057"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\bases_risk.zip",
		Sha1 = "A28BC9BE7F2AB1F594B97C41B0DBEFFC5B429C47"
	},
	new APTAT.Valkyrie.Client.ValkyrieFileInfo
	{
		Ip = "10.100.65.149",
		Path = "C:\\Users\\User\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\ICU.nfo",
		Sha1 = "82F206FCE3895CBE69CD6C9E4DB29EA7BC4B9FEE"
	}
};
            return badFiles;
        }




    }
}
