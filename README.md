# AspNetMvc5 web application Calculator

Веб приложение калькулятор на ASP.NET MVC язык C#.

Приложение выполняет сохранение вычислений в базу данных, а именно:
  * введённые пользователем в калькуляторе выражения/формулы
  * дата/время их вычисления
  * IP-адрес клиента

В интерфейсе приложения отображается история предыдущих вычислений за текущие сутки и (только) для текущего IP-адреса клиента, позволяющая повторить ранее выполненные вычисления.

Для взаимодействия с БД используется технология ADO.NET.
В качестве СУБД используется Microsoft SQL Server и база данных LocalDB (SQL Server Express), 
т.е. приложение не требует разворачивания БД в Microsoft SQL Server.
