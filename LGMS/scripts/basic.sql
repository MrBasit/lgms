Use [lgcd_lgms_dev01];
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE';


select * from employees;
select * from departments;
select * from designations;
select * from employeestatus;

update employees set BirthDate = '2024-09-30 15:06:13.3170000' where id=1;
update employees set StatusId = 1 where id=3;