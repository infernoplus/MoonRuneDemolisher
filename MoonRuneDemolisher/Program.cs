using System;
using System.Collections.Generic;
using System.IO;
using SoulsFormats;
using Google.Cloud.Translation.V2;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using System.ComponentModel.Design;
using System.Xml.Serialization;

namespace MoonRuneDemolisher
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1) { help();  return; }
            switch (args[0])
            {
                case "params": { translateParams(args[1], args[2]); break; }
                case "paramdefs": { translateParamDefs(args[1]); break; }
                case "msbs": { translateMsbs(args[1]); break; }
                default: { help(); break; }
            }

        }

        static void translateParams(string a, string b)
        {
            string paramDir = a.EndsWith("\\") ? a.Substring(a.Length - 1, 1) : a;
            string paramDefDir = b.EndsWith("\\") ? b.Substring(b.Length - 1, 1) : b;
            string[] paramFileList = Directory.GetFiles(paramDir);
            string[] paramDefFileList = Directory.GetFiles(paramDefDir);
            List<string> paramFileNameList = new List<string>();
            List<PARAMDEF> paramDefs = new List<PARAMDEF>();
            List<PARAM> paramaroos = new List<PARAM>();

            Console.WriteLine("### " + paramDir);
            Console.WriteLine("### " + paramDefDir + "\n");

            for (int i = 0; i < paramFileList.Length; i++)
            {
                string fn = paramFileList[i].Substring(paramDir.Length + 1, paramFileList[i].Length - (paramDir.Length + 1));
                paramFileNameList.Add(fn);
                paramaroos.Add(PARAM.Read(File.ReadAllBytes(paramFileList[i])));
            }

            for (int i = 0; i < paramDefFileList.Length; i++)
            {
                paramDefs.Add(PARAMDEF.Read(File.ReadAllBytes(paramDefFileList[i])));
            }

            for (int i = 0; i < paramaroos.Count; i++)
            {
                PARAM p = paramaroos[i];
                for (int j = 0; j < paramDefs.Count; j++)
                {
                    PARAMDEF pd = paramDefs[j];

                    if (p.ParamType.Equals(pd.ParamType))
                    {
                        p.ApplyParamdef(pd);
                    }
                }
            }

            TranslationClient client = TranslationClient.Create(GoogleCredential.FromFile("C:\\Users\\dmtin\\google-translate-api-key.txt"));

            for (int i = 0; i < paramaroos.Count; i++)
            {
                Console.WriteLine("\n\n\n\n==================" + paramaroos[i].ParamType + "==================");
                for (int j = 0; j < paramaroos[i].Rows.Count; j++)
                {
                    PARAM.Row row = paramaroos[i].Rows[j];
                    try
                    {
                        if (row.Name != null && !row.Name.Trim().Equals("") && !row.Name.Trim().Equals("0"))
                        {
                            TranslationResult response = client.TranslateText(row.Name, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                            if (response != null && response.TranslatedText != null && response.TranslatedText.Trim().Length > 0)
                            {
                                row.Name = response.TranslatedText;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("EXCEPTION :: " + ex.Message); }
                    Console.WriteLine(row.ID + ":: " + row.Name);
                }
            }

            Directory.CreateDirectory(paramDir + "\\translated\\");
            for (int i = 0; i < paramaroos.Count; i++)
            {
                string outPath = paramDir + "\\translated\\" + paramFileNameList[i];
                byte[] outData = paramaroos[i].Write();
                File.WriteAllBytes(outPath, outData);
            }

            Console.WriteLine("\n\n Done!");
        }

        static void translateMsbs(string a)
        {
            string msbDir = a.EndsWith("\\") ? a.Substring(a.Length - 1, 1) : a;
            string[] msbFileList = Directory.GetFiles(msbDir);
            List<string> msbFileNameList = new List<string>();
            List<MSB1> msbs = new List<MSB1>();

            Console.WriteLine("### " + msbDir);

            for (int i = 0; i < msbFileList.Length; i++)
            {
                string fn = msbFileList[i].Substring(msbDir.Length + 1, msbFileList[i].Length - (msbDir.Length + 1));
                msbFileNameList.Add(fn);
                msbs.Add(MSB1.Read(File.ReadAllBytes(msbFileList[i])));
            }

            TranslationClient client = TranslationClient.Create(GoogleCredential.FromFile("C:\\Users\\dmtin\\google-translate-api-key.txt"));

            /* I could also translate the region names but I'd have to build a map of all the original names -> translated names and apply the new names to the right events */
            /* A lot of work and potentially buggy so I'm not going to do it right now. */

            for (int i=0;i<msbs.Count;i++)
            {
                MSB1 msb = msbs[i];
                Console.WriteLine("\n\n\n\n==================" + msbFileNameList[i] + "==================");

                Console.WriteLine("\n\n#### EventType: Environment ####");
                for(int j=0;j<msb.Events.Environments.Count;j++)
                {
                    MSB1.Event.Environment evto = msb.Events.Environments[j];
                    try
                    {
                        if (evto.Name != null && !evto.Name.Trim().Equals(""))
                        {
                            TranslationResult response = client.TranslateText(evto.Name, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                            if (response != null && response.TranslatedText != null && response.TranslatedText.Trim().Length > 0)
                            {
                                evto.Name = response.TranslatedText;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("EXCEPTION :: " + ex.Message); }
                    Console.WriteLine(evto.EventID + ":: " + evto.Name);
                }

                Console.WriteLine("\n\n#### EventType: Generator ####");
                for (int j = 0; j < msb.Events.Generators.Count; j++)
                {
                    MSB1.Event.Generator evto = msb.Events.Generators[j];
                    try
                    {
                        if (evto.Name != null && !evto.Name.Trim().Equals(""))
                        {
                            TranslationResult response = client.TranslateText(evto.Name, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                            if (response != null && response.TranslatedText != null && response.TranslatedText.Trim().Length > 0)
                            {
                                evto.Name = response.TranslatedText;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("EXCEPTION :: " + ex.Message); }
                    Console.WriteLine(evto.EventID + ":: " + evto.Name);
                }

                Console.WriteLine("\n\n#### EventType: Navmesh ####");
                for (int j = 0; j < msb.Events.Navmeshes.Count; j++)
                {
                    MSB1.Event.Navmesh evto = msb.Events.Navmeshes[j];
                    try
                    {
                        if (evto.Name != null && !evto.Name.Trim().Equals(""))
                        {
                            TranslationResult response = client.TranslateText(evto.Name, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                            if (response != null && response.TranslatedText != null && response.TranslatedText.Trim().Length > 0)
                            {
                                evto.Name = response.TranslatedText;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("EXCEPTION :: " + ex.Message); }
                    Console.WriteLine(evto.EventID + ":: " + evto.Name);
                }

                Console.WriteLine("\n\n#### EventType: Light ####");
                for (int j = 0; j < msb.Events.Lights.Count; j++)
                {
                    MSB1.Event.Light evto = msb.Events.Lights[j];
                    try
                    {
                        if (evto.Name != null && !evto.Name.Trim().Equals(""))
                        {
                            TranslationResult response = client.TranslateText(evto.Name, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                            if (response != null && response.TranslatedText != null && response.TranslatedText.Trim().Length > 0)
                            {
                                evto.Name = response.TranslatedText;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("EXCEPTION :: " + ex.Message); }
                    Console.WriteLine(evto.EventID + ":: " + evto.Name);
                }

                Console.WriteLine("\n\n#### EventType: Message ####");
                for (int j = 0; j < msb.Events.Messages.Count; j++)
                {
                    MSB1.Event.Message evto = msb.Events.Messages[j];
                    try
                    {
                        if (evto.Name != null && !evto.Name.Trim().Equals(""))
                        {
                            TranslationResult response = client.TranslateText(evto.Name, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                            if (response != null && response.TranslatedText != null && response.TranslatedText.Trim().Length > 0)
                            {
                                evto.Name = response.TranslatedText;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("EXCEPTION :: " + ex.Message); }
                    Console.WriteLine(evto.EventID + ":: " + evto.Name);
                }

                Console.WriteLine("\n\n#### EventType: ObjAct ####");
                for (int j = 0; j < msb.Events.ObjActs.Count; j++)
                {
                    MSB1.Event.ObjAct evto = msb.Events.ObjActs[j];
                    try
                    {
                        if (evto.Name != null && !evto.Name.Trim().Equals(""))
                        {
                            TranslationResult response = client.TranslateText(evto.Name, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                            if (response != null && response.TranslatedText != null && response.TranslatedText.Trim().Length > 0)
                            {
                                evto.Name = response.TranslatedText;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("EXCEPTION :: " + ex.Message); }
                    Console.WriteLine(evto.EventID + ":: " + evto.Name);
                }

                Console.WriteLine("\n\n#### EventType: SFX ####");
                for (int j = 0; j < msb.Events.SFX.Count; j++)
                {
                    MSB1.Event.SFX evto = msb.Events.SFX[j];
                    try
                    {
                        if (evto.Name != null && !evto.Name.Trim().Equals(""))
                        {
                            TranslationResult response = client.TranslateText(evto.Name, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                            if (response != null && response.TranslatedText != null && response.TranslatedText.Trim().Length > 0)
                            {
                                evto.Name = response.TranslatedText;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("EXCEPTION :: " + ex.Message); }
                    Console.WriteLine(evto.EventID + ":: " + evto.Name);
                }

                Console.WriteLine("\n\n#### EventType: Sound ####");
                for (int j = 0; j < msb.Events.Sounds.Count; j++)
                {
                    MSB1.Event.Sound evto = msb.Events.Sounds[j];
                    try
                    {
                        if (evto.Name != null && !evto.Name.Trim().Equals(""))
                        {
                            TranslationResult response = client.TranslateText(evto.Name, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                            if (response != null && response.TranslatedText != null && response.TranslatedText.Trim().Length > 0)
                            {
                                evto.Name = response.TranslatedText;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("EXCEPTION :: " + ex.Message); }
                    Console.WriteLine(evto.EventID + ":: " + evto.Name);
                }

                Console.WriteLine("\n\n#### EventType: SpawnPoint ####");
                for (int j = 0; j < msb.Events.SpawnPoints.Count; j++)
                {
                    MSB1.Event.SpawnPoint evto = msb.Events.SpawnPoints[j];
                    try
                    {
                        if (evto.Name != null && !evto.Name.Trim().Equals(""))
                        {
                            TranslationResult response = client.TranslateText(evto.Name, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                            if (response != null && response.TranslatedText != null && response.TranslatedText.Trim().Length > 0)
                            {
                                evto.Name = response.TranslatedText;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("EXCEPTION :: " + ex.Message); }
                    Console.WriteLine(evto.EventID + ":: " + evto.Name);
                }

                Console.WriteLine("\n\n#### EventType: Treasure ####");
                for (int j = 0; j < msb.Events.Treasures.Count; j++)
                {
                    MSB1.Event.Treasure evto = msb.Events.Treasures[j];
                    try
                    {
                        if (evto.Name != null && !evto.Name.Trim().Equals(""))
                        {
                            TranslationResult response = client.TranslateText(evto.Name, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                            if (response != null && response.TranslatedText != null && response.TranslatedText.Trim().Length > 0)
                            {
                                evto.Name = response.TranslatedText;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("EXCEPTION :: " + ex.Message); }
                    Console.WriteLine(evto.EventID + ":: " + evto.Name);
                }

                Console.WriteLine("\n\n#### EventType: Wind ####");
                for (int j = 0; j < msb.Events.Wind.Count; j++)
                {
                    MSB1.Event.Wind evto = msb.Events.Wind[j];
                    try
                    {
                        if (evto.Name != null && !evto.Name.Trim().Equals(""))
                        {
                            TranslationResult response = client.TranslateText(evto.Name, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                            if (response != null && response.TranslatedText != null && response.TranslatedText.Trim().Length > 0)
                            {
                                evto.Name = response.TranslatedText;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("EXCEPTION :: " + ex.Message); }
                    Console.WriteLine(evto.EventID + ":: " + evto.Name);
                }
            }

            Directory.CreateDirectory(msbDir + "\\translated\\");
            for (int i = 0; i < msbs.Count; i++)
            {
                string outPath = msbDir + "\\translated\\" + msbFileNameList[i];
                byte[] outData = msbs[i].Write();
                File.WriteAllBytes(outPath, outData);
            }

            Console.WriteLine("\n\n Done!");
        }

        static void translateParamDefs(string a)
        {
            string paramDefDir = a.EndsWith("\\") ? a.Substring(a.Length - 1, 1) : a;
            string[] paramDefFileList = Directory.GetFiles(paramDefDir);
            List<string> paramDefFileNameList = new List<string>();
            List<PARAMDEF> paramDefs = new List<PARAMDEF>();

            Console.WriteLine("### " + paramDefDir);

            for (int i = 0; i < paramDefFileList.Length; i++)
            {
                string fn = paramDefFileList[i].Substring(paramDefDir.Length + 1, paramDefFileList[i].Length - (paramDefDir.Length + 1));
                paramDefFileNameList.Add(fn);
                paramDefs.Add(PARAMDEF.Read(File.ReadAllBytes(paramDefFileList[i])));
            }

            TranslationClient client = TranslationClient.Create(GoogleCredential.FromFile("C:\\Users\\dmtin\\google-translate-api-key.txt"));

            for (int i = 0; i < paramDefs.Count; i++)
            {
                PARAMDEF pd = paramDefs[i];
                Console.WriteLine("\n\n\n\n==================" + pd.ParamType + "==================");

                for (int j=0; j<pd.Fields.Count;j++)
                {
                    PARAMDEF.Field field = pd.Fields[j];
                    try
                    {
                        TranslationResult responseA = client.TranslateText(field.DisplayName, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                        if (responseA != null && responseA.TranslatedText != null && responseA.TranslatedText.Trim().Length > 0)
                        {
                            field.DisplayName = responseA.TranslatedText;
                        }

                        TranslationResult responseB = client.TranslateText(field.Description, LanguageCodes.English, LanguageCodes.Japanese); // Translate request
                        if (responseB != null && responseB.TranslatedText != null && responseB.TranslatedText.Trim().Length > 0)
                        {
                            field.Description = responseB.TranslatedText;
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("EXCEPTION :: " + ex.Message); }
                    Console.WriteLine(field.DisplayName + ":: " + field.Description);
                }
            }

            Directory.CreateDirectory(paramDefDir + "\\translated\\");
            for (int i = 0; i < paramDefs.Count; i++)
            {
                string outPath = paramDefDir + "\\translated\\" + paramDefFileNameList[i];
                byte[] outData = paramDefs[i].Write();
                File.WriteAllBytes(outPath, outData);
            }
        }

        static void help()
        {
            Console.WriteLine("MoonRuneDemolisher - By Inferno - Uses SoulsFormat by TKGP");
            Console.WriteLine("Commands:");
            Console.WriteLine("  params <path to unpacked param files> <path to unpacked paramdef files>");
            Console.WriteLine("  msbs <path mapstudio folder with all the msb files>");
            Console.WriteLine("  paramdefs <path to unpacked paramdef files>");
        }
    }
}
