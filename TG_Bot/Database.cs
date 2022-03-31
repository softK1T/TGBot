using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.SQLite.Generic;

namespace TG_Bot
{
    public class Database
    {
        public int result;
        public string connectionstring = "Data Source = TelegramDB.db; Version=3;";
        public bool GetUser(long id)
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionstring))
            {
                con.Open();
                string query = ("SELECT name FROM users WHERE id='" + id.ToString() + "'");
                using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                {
                    SQLiteDataReader dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        Console.WriteLine("Looking for " + id + "\nResult: user has already registered.");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("User not registered.");
                        return false;
                    }
                }
            }
        }
        public void NewUserQuery(long userid, string name)
        {
            if (!GetUser(userid))
            {
                using (SQLiteConnection con = new SQLiteConnection(connectionstring))
                {
                    con.Open();
                    string queryinsert = "INSERT INTO users ('id', 'name', 'state') VALUES (@id, @name, @state)";
                    using (SQLiteCommand cmd = new SQLiteCommand(queryinsert, con))
                    {
                        cmd.Parameters.AddWithValue("@id", userid);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@state", "Menu");
                        result = cmd.ExecuteNonQuery();
                        Console.WriteLine("New user has been registered. User name: " + name + "\nUser ID: " + userid + "\nResult: " + result);
                    }
                }
            }
        }
        public CurrentState GetState(long userid)
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionstring))
            {
                con.Open();
                string query = ("SELECT state FROM users WHERE id='" + userid.ToString() + "'");
                using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                {
                    SQLiteDataReader dr = cmd.ExecuteReader();
                    string res = "";
                    if (dr.HasRows)
                        while (dr.Read())
                        {
                            res = dr["state"].ToString();
                        }
                    Enum.TryParse(res, out CurrentState curst);
                    Console.WriteLine("State for " + userid + " is: " + curst.ToString());
                    return curst;
                }
            }
        }
        public void UpdateState(long userid, CurrentState state)
        {
            string query = "UPDATE users SET state = '" + state.ToString() + "' WHERE id = " + userid + ";";
            using (SQLiteConnection con = new SQLiteConnection(connectionstring))
            {
                con.Open();                
                using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", userid);
                    cmd.Parameters.AddWithValue("@state", state.ToString());
                    try
                    {
                        result = cmd.ExecuteNonQuery();
                        Console.WriteLine("State has been changed for: " + userid + "\nNew state: " + state.ToString() + ". Result: " + result);
                    }
                    catch (Exception e)
                    {
                        BotTG.LogEx(e, "UPDATE STATE ");
                        Console.Error.WriteLine();
                    }                    
                }
            }
        }
        public void UpdateName(long userid, string name)
        {
            string query = "UPDATE users SET tempname = '" + name + "' WHERE id = " + userid + ";";
            using (SQLiteConnection con = new SQLiteConnection(connectionstring))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", userid);
                    cmd.Parameters.AddWithValue("@tempname", name.ToString());
                    result = cmd.ExecuteNonQuery();
                    Console.WriteLine("Temporary note name has been changed for: " + userid + "\nResult: " + result);
                }
            }
        }
        public string GetTempName(long userid)
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionstring))
            {
                con.Open();
                string query = ("SELECT tempname FROM users WHERE id='" + userid + "'");
                using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                {
                    SQLiteDataReader dr = cmd.ExecuteReader();
                    string res = "";
                    if (dr.HasRows)
                        while (dr.Read())
                            res = dr["tempname"].ToString();
                    return res;
                }
            }
        }
        public void AddNote(long userid, string name, string content)
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionstring))
            {
                con.Open();
                string queryinsert = "INSERT INTO notes ('id', 'name', 'contain') VALUES (@id, @name, @contain)";
                using (SQLiteCommand cmd = new SQLiteCommand(queryinsert, con))
                {
                    cmd.Parameters.AddWithValue("@id", userid);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@contain", content);
                    result = cmd.ExecuteNonQuery();
                    Console.WriteLine("New note has been added. User name: " + name + "\nUser ID: " + userid + "\nNote name: " + name + "\nNote content: " +
                        content + "\nResult: " + result);
                    BotTG.Bot.SendTextMessageAsync(userid, "New note has been created. Note name: " + name + ". Note content: " + content);
                }
            }
        }
        public void GetNotes(Telegram.Bot.Types.Message message)
        {
            int i = 1;
            long uid = message.From.Id;
            using (SQLiteConnection con = new SQLiteConnection(connectionstring))
            {
                con.Open();
                string query = ("SELECT name, contain, rowid FROM notes WHERE id='" + uid.ToString() + "'");
                using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                {
                    SQLiteDataReader dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        Console.WriteLine("Notes: ");
                        while (dr.Read())
                        {
                            Console.Write(dr.GetValue(0).ToString() + " ");
                            BotTG.SendMessageWithReply(uid, "Note ID " + dr.GetValue(2).ToString() + ":\n" + dr.GetValue(0).ToString() + "\n" + dr.GetValue(1).ToString());
                            i++;
                            System.Threading.Thread.Sleep(100);
                        }
                        dr.Close();
                        Console.WriteLine("");
                    }
                }
            }
        }
        public void DeleteNote(long userid, string name, string contain, string rowid)
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionstring))
            {
                con.Open();
                string query = $"DELETE FROM notes WHERE id={userid} AND rowid={rowid}";
                Console.WriteLine(query);
                using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                {
                    var trans = con.BeginTransaction();
                    cmd.Connection = con;
                    try
                    {
                        result = cmd.ExecuteNonQuery();
                        trans.Commit();
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message, "DELETE NOTE");
                    }
                    Console.WriteLine($"Deleted:{result}");
                }

            }
        }

        public void UpdateNote(long userid, string rowid, string name)
        {
            string query = "UPDATE note SET name= '" + name + "' WHERE id = " + userid + ";";
            using (SQLiteConnection con = new SQLiteConnection(connectionstring))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", userid);
                    cmd.Parameters.AddWithValue("@tempname", name.ToString());
                    result = cmd.ExecuteNonQuery();
                    Console.WriteLine("Temporary note name has been changed for: " + userid + "\nResult: " + result);
                }
            }
        }
        public void DatabaseIni()
        {
            SQLiteConnection con = new SQLiteConnection(connectionstring);
            con.Open();
        }
        //public void dataShow()
        //{
        //    //var con = new SQLiteConnection(connectionstring);
        //    con.Open();
        //    string query = "SELECT * FROM notes";
        //    var cmd = new SQLiteCommand(query, con);
        //    dr = cmd.ExecuteReader();
        //    if (dr.HasRows)
        //        while (dr.Read())
        //            Console.WriteLine("Name: {0}\nContent: {1}", dr["name"], dr["contain"]);
        //}
    }
}
