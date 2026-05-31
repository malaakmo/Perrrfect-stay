# Perrrfect Stay

Desktop app for a cat café and cat hotel - project for Inspiration Lab at Thomas More University.  
Malaak Timraz (Frontend) & Vasilina Tomanova (Backend) - DSPS B 2026



Perrrfect Stay is a Windows desktop application that manages two things at once: a cat café where people can book tables and spend time with cats, and a cat hotel where owners can leave their cats while they travel. On top of that, the app has a cat sitting feature where owners can find and book trusted sitters.

The app has three types of users. Regular users can create an account, log in, browse cats, make bookings, submit adoption or sponsorship requests, and leave reviews. Sitters can receive and manage sitting requests through their dashboard. Admins have full control — they can manage all users, approve or reject requests, view platform stats, and download reports.

On the backend side, every feature is covered by a REST API built in ASP.NET Core. The API handles user authentication with BCrypt password hashing, generates PDFs for booking history, creates QR codes for café reservations, and connects to a MySQL database via XAMPP. All endpoints are documented and testable through Swagger.

The frontend is a WPF desktop app built in C# that connects directly to the same MySQL database. It has screens for login, registration, browsing cats, making hotel and café bookings, sponsoring cats, viewing bookings, and an admin panel.



To run it you need XAMPP with MySQL on port 3308 and Visual Studio.

Open `PerrrfectStayAPI.slnx` for the backend - it runs at `https://localhost:7204/swagger`. Open `Perrrfect stay.slnx` for the WPF frontend. Both need the connection string pointing to `perrrfect_stay` on localhost.

If MySQL auth fails on first run, execute this in phpMyAdmin:

```sql
ALTER USER 'root'@'localhost' IDENTIFIED VIA mysql_native_password USING PASSWORD('root');
FLUSH PRIVILEGES;
```

Built with WPF, ASP.NET Core, MySQL, BCrypt, iText7, and QRCoder.
