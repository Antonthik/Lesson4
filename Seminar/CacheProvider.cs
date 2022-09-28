using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Security.Cryptography;

namespace Seminar
{

    public abstract class BaseCacheProviderException : Exception { }
    public class SerializeDataCacheProviderException : BaseCacheProviderException { }//пользовательское исключение
    public class DeserializeDataCacheProviderException : BaseCacheProviderException { }//пользовательское исключение

    public class ProtectCacheProviderException : BaseCacheProviderException { }//пользовательское исключение
    public class UnprotectCacheProviderException : BaseCacheProviderException { }//пользовательское исключение

    //Шифрование и дешифрование данных
    public class CacheProvider
    {
        static byte[] additionalEntropy = { 5, 3, 7, 1, 5 };//секретный ключ

        public void CacheConnections(List<ConnectionString> connections)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ConnectionString>));
                using MemoryStream memoryStream = new MemoryStream();
                using XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);//работает с потоком в памяти
                xmlSerializer.Serialize(xmlTextWriter, connections);//контент в памяти
                byte[] protectedData = Protect(memoryStream.ToArray());//получаем бинарное представление и шифруем данные
                File.WriteAllBytes("data.protected", protectedData);//пишем в файл

            }
            catch (Exception e)
            {
                Console.WriteLine("Serialize data error.");
            }
        }

        //Шифрование
        private byte[] Protect(byte[] data)
        {
            try
            {
                return ProtectedData.Protect(data, additionalEntropy, DataProtectionScope.CurrentUser);//CurrentUser-шифруем на уровне пользователя, только под ним можно расшифровать
            }
            catch (Exception e)
            {
                Console.WriteLine("Protected error.");
                throw new SerializeDataCacheProviderException();
                //return null;
            }
        }

        
        public List<ConnectionString> GetConnectionsFromCache()
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ConnectionString>));
                byte[] protectedData = File.ReadAllBytes("data.protected");//читаем из файла
                byte[] data = Unprotect(protectedData);//дешифруем
                return (List<ConnectionString>)xmlSerializer.Deserialize(new MemoryStream(data));
            }
            catch (Exception e)
            {
                Console.WriteLine("Deserialize data error.");
                //return null;
                throw new DeserializeDataCacheProviderException();
            }
        }

        //Дешифрование
        private byte[] Unprotect(byte[] data)
        {
            try
            {
                return ProtectedData.Unprotect(data, additionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unprotect error.");
                //return null;
                throw new UnprotectCacheProviderException();
            }
        }

    }
}

