select * from employees where identityuserid is not null;

select * from aspnetusers;
select * from AspNetRoles;
select * from AspNetUserRoles

delete from AspNetRoles;

update employees set identityuserid = null where id=45;