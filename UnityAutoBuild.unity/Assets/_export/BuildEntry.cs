﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace FIX.UnityAutoBuild {
    public class BuildEntry {
        protected static void __dieWithException (Exception e, int ec) {
            Console.Error.WriteLine(e);
            EditorApplication.Exit(ec);
        }

        public static void Run () {
            BuildPlayerOptions opt = new BuildPlayerOptions();

            try {
                var m = LongOptParser.ParseLongOpts(Environment.GetCommandLineArgs());
                var scene_list = new List<string>();
                Func<string, string> get_req_key = (string k) => {
                    string ret;

                    if (!m.TryGetValue(k, out ret)) {
                        throw new ArgumentException(string.Format("'{0}' option required.", k));
                    }
                    if (ret == null) {
                        throw new ArgumentException(string.Format("value required: '{0}' option.", k));
                    }

                    return ret;
                };

                switch (get_req_key("fix.uab.base")) {
                    case "WIN_X86": opt = BuildConfig.WIN_X86; break;
                    case "WIN_X64": opt = BuildConfig.WIN_X64; break;
                    case "LINUX": opt = BuildConfig.LINUX; break;
                    case "LINUX_HEADLESS": opt = BuildConfig.LINUX_HEADLESS; break;
                    case "ANDROID": opt = BuildConfig.ANDROID; break;
                    case "IOS": opt = BuildConfig.IOS; break;
                    case "OSX": opt = BuildConfig.OSX; break;
                    default: throw new Exception();
                }

                opt.locationPathName = get_req_key("fix.uab.path");
                foreach (var s in EditorBuildSettings.scenes) {
                    if (s.enabled) {
                        scene_list.Add(s.path);
                    }
                }
                opt.scenes = scene_list.ToArray();


                foreach (var p in m) {
                    if (p.Key.StartsWith("fix.uab.build.prop.")) {
                        var v = (BuildOptions)Enum.Parse(typeof(BuildOptions), p.Key.Substring(19));

                        if (bool.Parse(p.Value)) {
                            opt.options |= v;
                        }
                        else {
                            opt.options &= ~v;
                        }
                    }
                }
            }
            catch (ArgumentException e) {
                __dieWithException(e, 2);
            }
            catch (Exception e) {
                __dieWithException(e, 1);
            }

            try {
                var report = BuildPipeline.BuildPlayer(opt);

                Console.Out.WriteLine("Build took {0:F3}s",
                   (report.summary.buildEndedAt - report.summary.buildStartedAt).Seconds);

                if (report.summary.result == BuildResult.Succeeded) {
                    EditorApplication.Exit(0);
                }
                else {
                    Console.Error.WriteLine("Build not successful! Result: {0}",
                        report.summary.result.ToString());
                }
            }
            catch (Exception e) {
                __dieWithException(e, 1);
            }

            EditorApplication.Exit(1);
        }
    }
}
#endif
