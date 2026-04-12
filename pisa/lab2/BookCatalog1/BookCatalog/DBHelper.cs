// DBHelper.cs — корень проекта, глобальное пространство имён (без namespace)
using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

public static class DBHelper
{
    private static string ConnectionString
    {
        get { return ConfigurationManager.ConnectionStrings["BookCatalogDB"].ConnectionString; }
    }

    // ── Хранимые процедуры ───────────────────────────────────────────────────

    public static int ExecuteNonQuery(string storedProc, params SqlParameter[] parameters)
    {
        using (SqlConnection conn = new SqlConnection(ConnectionString))
        using (SqlCommand cmd = new SqlCommand(storedProc, conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }
    }

    public static DataTable ExecuteQuery(string storedProc, params SqlParameter[] parameters)
    {
        DataTable result = new DataTable();
        using (SqlConnection conn = new SqlConnection(ConnectionString))
        using (SqlCommand cmd = new SqlCommand(storedProc, conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                conn.Open();
                adapter.Fill(result);
            }
        }
        return result;
    }

    public static object ExecuteScalar(string storedProc, params SqlParameter[] parameters)
    {
        using (SqlConnection conn = new SqlConnection(ConnectionString))
        using (SqlCommand cmd = new SqlCommand(storedProc, conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            conn.Open();
            return cmd.ExecuteScalar();
        }
    }

    // ── Прямые SQL-запросы ───────────────────────────────────────────────────

    public static DataTable ExecuteQueryText(string sql, params SqlParameter[] parameters)
    {
        DataTable result = new DataTable();
        using (SqlConnection conn = new SqlConnection(ConnectionString))
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.CommandType = CommandType.Text;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                conn.Open();
                adapter.Fill(result);
            }
        }
        return result;
    }

    public static int ExecuteNonQueryText(string sql, params SqlParameter[] parameters)
    {
        using (SqlConnection conn = new SqlConnection(ConnectionString))
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.CommandType = CommandType.Text;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }
    }

    // ── Утилиты ──────────────────────────────────────────────────────────────

    public static string HashPassword(string password)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}


