import { Component, Injector, ViewEncapsulation } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { MenuItem } from '@shared/layout/menu-item';

@Component({
    templateUrl: './sidebar-nav.component.html',
    selector: 'sidebar-nav',
    styleUrls: ['./sidebar-nav.component.less'],
    encapsulation: ViewEncapsulation.None
})
export class SideBarNavComponent extends AppComponentBase {

    menuItems: MenuItem[] = [
        
        new MenuItem('主页', '', 'fa fa-home', '/app/device/home'),
        new MenuItem('测试页', '', 'fa fa-file-text-o', '/app/test'),

        new MenuItem('系统管理', '', 'fa fa-bars', '', [
            new MenuItem(this.l('Users'), 'Pages.Users', 'fa fa-users', '/app/users'),
            new MenuItem(this.l('Roles'), 'Pages.Roles', 'fa fa-user-secret', '/app/roles'),
            new MenuItem(this.l('About'), '', 'fa fa-info-circle', '/app/about'),
        ]),
        
        // new MenuItem('心跳记录', '', 'fa fa-file-text-o', '/app/device/device-list'),
        // new MenuItem('传感器波动记录', 'fa fa-file-text-o', '', '/app/device/device-list'),
        // new MenuItem('指令记录', '', 'fa fa-file-text-o', '/app/device/device-list'),

        // new MenuItem(this.l('Tenants'), 'Pages.Tenants', 'business', '/app/tenants'),
        // new MenuItem(this.l('Users'), 'Pages.Users', 'fa fa-users', '/app/users'),
        // new MenuItem(this.l('Roles'), 'Pages.Roles', 'fa fa-user-secret', '/app/roles'),
        // new MenuItem(this.l('About'), '', 'fa fa-info-circle', '/app/about'),

        // new MenuItem(this.l('系统管理'), '', 'fa fa-bars', '', [
        //     new MenuItem(this.l('Tenants'), 'Pages.Tenants', 'business', '/app/tenants'),
        //     new MenuItem(this.l('Users'), 'Pages.Users', 'fa fa-users', '/app/users'),
        //     new MenuItem(this.l('Roles'), 'Pages.Roles', 'fa fa-user-secret', '/app/roles'),
        //     new MenuItem(this.l('About'), '', 'fa fa-info-circle', '/app/about'),
    
        // ]),

        // new MenuItem(this.l('MultiLevelMenu'), '', 'fa fa-bars', '', [
        //     new MenuItem('ASP.NET Boilerplate', '', '', '', [
        //         new MenuItem('Home', '', '', 'https://aspnetboilerplate.com/?ref=abptmpl'),
        //         new MenuItem('Templates', '', '', 'https://aspnetboilerplate.com/Templates?ref=abptmpl'),
        //         new MenuItem('Samples', '', '', 'https://aspnetboilerplate.com/Samples?ref=abptmpl'),
        //         new MenuItem('Documents', '', '', 'https://aspnetboilerplate.com/Pages/Documents?ref=abptmpl')
        //     ]),
        //     new MenuItem('ASP.NET Zero', '', '', '', [
        //         new MenuItem('Home', '', '', 'https://aspnetzero.com?ref=abptmpl'),
        //         new MenuItem('Description', '', '', 'https://aspnetzero.com/?ref=abptmpl#description'),
        //         new MenuItem('Features', '', '', 'https://aspnetzero.com/?ref=abptmpl#features'),
        //         new MenuItem('Pricing', '', '', 'https://aspnetzero.com/?ref=abptmpl#pricing'),
        //         new MenuItem('Faq', '', '', 'https://aspnetzero.com/Faq?ref=abptmpl'),
        //         new MenuItem('Documents', '', '', 'https://aspnetzero.com/Documents?ref=abptmpl')
        //     ])
        // ])
    ];

    constructor(
        injector: Injector
    ) {
        super(injector);
    }

    showMenuItem(menuItem): boolean {
        if (menuItem.permissionName) {
            return this.permission.isGranted(menuItem.permissionName);
        }

        return true;
    }
}
