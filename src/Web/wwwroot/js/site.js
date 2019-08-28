﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
Vue.prototype.$axios = axios;

var app = new Vue({
    data: {
        isCollapse: false,
        showOrHideMenuIcon: 'el-icon-s-fold',
        showOrHideMenuTitle: '收起',
        asideClass: 'aside-expand',
        ruleForm: {
            connectionString: '',
            outputPath: 'D:\\CodeGenerates',
            solutionName: '',
            modelsNamespace: '',
            viewmodelsNamespace: '',
            controllersNamespace: '',
            iRepositoriesNamespace: '',
            repositoriesNamespace: '',
            iServicesNamespace: '',
            servicesNamespace: '',
            tableData: []
        },
        rules: {
            connectionString: [
                { required: true, message: '请输入数据库连接字符串', trigger: 'blur' }
            ],
            outputPath: [
                { required: true, message: '请输入文件输出路径', trigger: 'blur' }
            ],
            solutionName: [
                { required: true, message: '请输入非数字开头的项目名称', trigger: 'blur' },
                {
                    pattern:/^[^0-9]+$/,message: '请输入非数字开头的项目名称', trigger: 'change'
                }
            ],                                    
            modelsNamespace: [                     
                { required: true, message: '请输入非数字开头的Model域名空间', trigger: 'blur' },
                {
                    pattern:/^[^0-9]+$/, message: '请输入非数字开头的Model域名空间', trigger: 'change'
                }
            ],                                    
            controllersNamespace: [               
                { required: true, message: '请输入非数字开头的Controller域名空间', trigger: 'blur' },
                {
                    pattern:/^[^0-9]+$/,message: '请输入非数字开头的Controller域名空间', trigger: 'change'
                }
            ],                                    
            iRepositoriesNamespace: [             
                { required: true, message: '请输入非数字开头的IRepository域名空间', trigger: 'blur'},
                {
                    pattern:/^[^0-9]+$/,message: '请输入非数字开头的IRepository域名空间', trigger: 'change'
                }
            ],                                    
            repositoriesNamespace: [              
                { required: true, message: '请输入非数字开头的Repository域名空间', trigger: 'blur'},
                {
                    pattern:/^[^0-9]+$/,message: '请输入非数字开头的Repository域名空间', trigger: 'change'
                }
            ],                                    
            iServicesNamespace: [                 
                { required: true, message: '请输入非数字开头的IService域名空间', trigger: 'blur'},
                {
                    pattern:/^[^0-9]+$/,message: '请输入非数字开头的IService域名空间', trigger: 'change'
                }
            ],                                    
            servicesNamespace: [                  
                { required: true, message: '请输入非数字开头的Service域名空间', trigger: 'blur'},
                {
                    pattern:/^[^0-9]+$/,message: '请输入非数字开头的Service域名空间', trigger: 'change'
                }
            ],                                    
            viewmodelsNamespace: [                
                { required: true, message: '请输入非数字开头的ViewModels域名空间', trigger: 'blur'},
                {
                    pattern:/^[^0-9]+$/,message: '请输入非数字开头的ViewModels域名空间', trigger: 'change'
                }
            ]
        },
        loading: false
    },
    mounted: function() {
        this.showOrHideMenu();
    },
    methods: {
        showOrHideMenu() {
            this.isCollapse = !this.isCollapse;
            if (this.isCollapse) {
                this.showOrHideMenuIcon = 'el-icon-s-unfold';
                this.showOrHideMenuTitle = '展开';
                this.asideClass = 'aside-collapse';
            } else {
                this.showOrHideMenuIcon = 'el-icon-s-fold';
                this.showOrHideMenuTitle = '收起';
                this.asideClass = 'aside-expand';
            }
        },
        forceUpdateInput() {
            this.$forceUpdate();
        },
        connectDatabase: function(url) {
            var that = app.ruleForm;
            if (!that.connectionString || that.connectionString.length === 0) {
                app.$message("请输入数据库连接字符串");
                return;
            }
            app.loading = true;
            app.$axios.get(url,
                {
                    params: {
                        connectionString: that.connectionString
                    }
                }).then(function(result) {
                app.loading = false;
                if (result.data) {
                    if (result.data.success) {
                        that.tableData = result.data.rows;
                        app.forceUpdateInput();
                    } else {
                        app.$message({
                            message:result.data.msg,
                            type:'error'
                        });
                    }
                }
            });
        },
        deleteRow(index, rows) {
            rows.splice(index, 1);
        },
        submitForm(postUrl) {
            var that = this;
            that.$refs["ruleForm"].validate((valid, obj) => {
                if (valid) {
                    if (!that.ruleForm.tableData || that.ruleForm.tableData.length === 0) {
                        that.$message({message:'请先获取数据表结构', type:'error'});
                        return false;
                    }
                    that.$axios.post(postUrl,that.ruleForm)
                        .then(function(result) {

                    });
                } else {
                    that.$message({message:'数据验证失败', type:'error'});
                    return false;
                }
            });

        },
        resetForm() {
            this.$refs["ruleForm"].resetFields();
        }
    },
    watch: {
        "ruleForm.solutionName": function(newValue, oldValue) {
            app.ruleForm.modelsNamespace = newValue + ".Models";
            app.ruleForm.viewmodelsNamespace = newValue + ".ViewModels";
            app.ruleForm.controllersNamespace = newValue + ".Controllers";
            app.ruleForm.iRepositoriesNamespace = newValue + ".IRepositories";
            app.ruleForm.repositoriesNamespace = newValue + ".Repositories";
            app.ruleForm.iServicesNamespace = newValue + ".IServices";
            app.ruleForm.servicesNamespace = newValue + ".Services";
            app.forceUpdateInput();
        }
    }
});