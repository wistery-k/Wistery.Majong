﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Threading;

using Codeplex.Data;

namespace Wistery.Majong
{
    public class FileMajongMain : MajongMain
    {
        private string path;
        private AI ai;

        public FileMajongMain(string path, AI ai)
        {
            this.path = path;
            this.ai = ai;
        }

        public void Main()
        {
            try
            {

                foreach (string line in File.ReadAllLines(path))
                {
                    Thread.Sleep(100);

                    Console.WriteLine(string.Format("<-\t{0}", line));
                    Log.WriteLine(string.Format("<-\t{0}", line));
                    var json = DynamicJson.Parse(line);

                    object response = null;

                    switch ((string)json.type)
                    {
                        case "hello":
                            response = Protocol.join("wistery_k", "default");
                            break;
                        case "start_game":
                            response = ai.onStartGame((int)json.id, (List<string>)json.names);
                            break;
                        case "start_kyoku":
                            response = ai.onStartKyoku(Pai.FromString((string)json.bakaze), (int)json.kyoku, (int)json.kyotaku, (int)json.honba, (int)json.oya, Pai.FromString((string)json.dora_marker), ((List<List<string>>)json.tehais).Select(tehai => tehai.Select(Pai.FromString).ToList()).ToList());
                            break;
                        case "tsumo":
                            response = ai.onTsumo((int)json.actor, Pai.FromString((string)json.pai));
                            break;
                        case "dahai":
                            response = ai.onDahai((int)json.actor, Pai.FromString((string)json.pai), (bool)json.tsumogiri);
                            break;
                        case "reach":
                            response = ai.onReach((int)json.actor);
                            break;
                        case "reach_accepted":
                            response = ai.onReachAccepted((int)json.actor, (List<int>)json.deltas, (List<int>)json.scores);
                            break;
                        case "pon":
                            response = ai.onPon((int)json.actor, (int)json.target, Pai.FromString((string)json.pai), ((List<string>)json.consumed).Select(Pai.FromString).ToList());
                            break;
                        case "chi":
                            response = ai.onChi((int)json.actor, (int)json.target, Pai.FromString((string)json.pai), ((List<string>)json.consumed).Select(Pai.FromString).ToList());
                            break;
                        case "kan":
                            response = ai.onKan((int)json.actor, (int)json.target, Pai.FromString((string)json.pai), ((List<string>)json.consumed).Select(Pai.FromString).ToList());
                            break;
                        case "hora":
                            response = ai.onHora((int)json.actor, 
                                                 (int)json.target, 
                                                 Pai.FromString((string)json.pai), 
                                                 ((List<string>)json.hora_tehais).Select(Pai.FromString).ToList(), 
                                                 ((List<List<dynamic>>)json.yakus).Select(st => new YakuN(YakuUtil.FromString((string)st[0]), (int)st[1])).ToList(), 
                                                 (int)json.fu, 
                                                 (int)json.fan, 
                                                 (int)json.hora_points, 
                                                 (List<int>)json.deltas, 
                                                 (List<int>)json.scores);
                            break;
                        case "ryukyoku":
                            response = ai.onRyukyoku((string)json.reason, ((List<List<string>>)json.tehais).Select(tehai => tehai.Select(Pai.FromString).ToList()).ToList(), (List<bool>)json.tenpais, (List<int>)json.deltas, (List<int>)json.scores);
                            break;
                        case "end_kyoku":
                            response = ai.onEndKyoku();
                            break;
                        case "end_game":
                            break;
                        case "error":
                            ai.onError(json.message);
                            Log.WriteLine(string.Format("error occurred: {0}", json.message));
                            break;
                        default:
                            Log.WriteLine(string.Format("unknown request: {0}", json.type));
                            response = Protocol.none();
                            break;
                    }

                    if (response == null) break;

                    string rawResponse = DynamicJson.Serialize(response);
                    Log.WriteLine(string.Format("->\t{0}", rawResponse));
                }
            }
            catch (Exception exn)
            {
                Log.WriteLine(exn.Message);
                Log.WriteLine(exn.StackTrace);
            }

            Console.WriteLine("finish");
        }
    }
}
