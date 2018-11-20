namespace Vintage.AppServices.DataAccessClasses
{
    public static class PracticeUser
    {
        public static bool AuthenticateUser(string userID, string password)
        {
            bool? authenticated = false;

            using (PatientsFirstDataContext dc = new PatientsFirstDataContext())
            {
                authenticated = dc.HimUserAuthenticate(userID, password);
            }

            return (authenticated.HasValue ? authenticated.Value : false);
        }
    }
}