﻿using System.Linq.Dynamic.Core;
using Application.Dto;
using Application.Dto.System.Department;
using Application.Permissions;
using Domain.Entity.System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Application.Service.System;

public class DepartmentService : MyCrudAppService<Department, DepartmentListOutput, DepartmentListOutput, Guid, GetDepartmentListInput, CreateOrEditDepartmentInput, CreateOrEditDepartmentInput>
{
    public DepartmentService(IRepository<Department, Guid> repository) : base(repository)
    {
        
    }

    protected override string GetListPolicyName => MyPermissions.Departments.Default;
    protected override string CreatePolicyName => MyPermissions.Departments.Create;
    protected override string UpdatePolicyName => MyPermissions.Departments.Update;
    protected override string DeletePolicyName => MyPermissions.Departments.Delete;

    public async  Task<List<TreeOptionDto>> GetOptions()
    {
        var queryable = await Repository.GetQueryableAsync();
        var departments = await queryable.ToListAsync();
        return ObjectMapper.Map<List<Department>, List<TreeOptionDto>>(departments.Where(d => d.ParentId == null).ToList());
    }

    public override async Task<PagedResultDto<DepartmentListOutput>> GetListAsync(GetDepartmentListInput input)
    {
        var result = await base.GetListAsync(input);
        return new PagedResultDto<DepartmentListOutput>(result.TotalCount,
            result.Items.Where(i => i.ParentId == null).ToList());
    }

    protected override async Task<IQueryable<Department>> CreateFilteredQueryAsync(GetDepartmentListInput input)
    {
        var queryable = await Repository.GetQueryableAsync();
        return queryable.Include(d => d.Children).WhereIf(!input.Filter.IsNullOrEmpty(), u => u.Name.Contains(input.Filter))
            .WhereIf(input.Status.HasValue, u => u.Status == input.Status).OrderBy(d => d.Sort);
    }

    protected override async Task CheckCreateValidateAsync(CreateOrEditDepartmentInput input)
    {
        await CheckNameExist(input.Name);
    }
    
    protected override async Task CheckUpdateValidateAsync (Guid id, CreateOrEditDepartmentInput input)
    {
        await CheckNameExist(input.Name, id);
    }

    private async Task CheckNameExist(string name, Guid? ignoreId = null)
    {
        var queryable = await Repository.GetQueryableAsync();
        var query = queryable.WhereIf(!name.IsNullOrEmpty(),  d => d.Name == name )
            .WhereIf(ignoreId.HasValue, r => r.Id != ignoreId.Value);
        
        if (await query.AnyAsync())
            throw new UserFriendlyException("部门名称已存在");
    }
}