
"use strict"; // to see code structure.. Ctrl+M, Ctrl+O 

$(document).ready(function () {

    let allInfoTables = []; // all tables from api fetch 
    let selectedInfoTables = ko.observableArray([]); // table(s) selected for query
    let isNewQuery = true;

    fetchJson("clientsettings.json", "GET", function (jsonResponse, response) { /* do something smart someday */ });
    fetchJson("api/TableSchemas", "GET", function (jsonResponse, response) {


        // fixup json data with our UI/binding needs
        jsonResponse.forEach(function (infoTable) {

            infoTable.cardClass = ko.observable("");
            infoTable.isSelectedQueryTable = ko.observable(false);
            infoTable.isPrimaryQueryTable = ko.observable(false);
            infoTable.isJoinedQueryTable = ko.observable(false);
            infoTable.excludeFromQuery = ko.observable(false);
            infoTable.selectedTableSeq = ko.observable(1);
            infoTable.parentTableSeq = ko.observable(0);

            allInfoTables.push(infoTable);
        });
        // get started on the info view
        selectRequired("#nav-info-tab").trigger('click');
    });

    //---------------------------------------------------------------------------------------------sql-info view
    let infoViewModel = undefined;
    selectRequired("#nav-info-tab").on("click", function (evt) {

        location.hash = "info";

        if (infoViewModel) return; // already been here? nothing to do?

        // ko viewmodel
        function InfoTablesViewModel(allInfoTables, selectedInfoTables) {

            const self = this;

            // cannot bind to these.. private?
            let allSchemas = [],
                currentSchema = undefined,
                isQueryBaseEstablished = false,
                tableSeq = 888;

            // get unique schema name(s)
            allInfoTables.forEach(function (infoTable) {
                allSchemas.push(infoTable.table_schema);
            });
            self.infoSchemas = [...new Set(allSchemas)];
            self.infoTables = allInfoTables;

            // functions
            self.setSchemaContext = function (schema) {
                currentSchema = schema;
                return currentSchema;
            };
            self.getTables = function () {
                return self.infoTables.filter(function (item) {
                    return item.table_schema === currentSchema;
                });
            };

            // event handlers
            self.onTableClick = function (infoTable, event) {

                if (isQueryBaseEstablished) {
                    addToQuery(infoTable);
                } else {
                    initiateNewQuery(infoTable);
                }
            };
            self.onLockClick = function (data) {

                self.onTableClick(data, null);
            };
            self.onResetQueryClick = function () {

                isNewQuery = true;
                selectedInfoTables([]);
                isQueryBaseEstablished = false;
                self.infoTables.forEach(function (infoTable) {
                    infoTable.isPrimaryQueryTable(false);
                    infoTable.isSelectedQueryTable(false);
                    infoTable.isJoinedQueryTable(false);
                    setCardClass(infoTable);
                });
            };

            // private stuff
            function setCardClass(infoTable) {

                if (infoTable.isSelectedQueryTable() === false) {
                    infoTable.cardClass("");
                    return;
                }

                let parentTableSeq = infoTable.parentTableSeq();
                if (infoTable.isPrimaryQueryTable() === true) {
                    infoTable.cardClass("flip-card-selected");
                } else {
                    infoTable.cardClass(parentTableSeq === 1 ? "flip-card-related1" : "flip-card-related2");
                }
            }
            function addToQuery(infoTable) {

                infoTable.isPrimaryQueryTable(false);

                let match = ko.utils.arrayFirst(selectedInfoTables(), function (item) {
                    // is this table alread selected?
                    return item.table_name === infoTable.table_name;
                });
                if (match) {

                    self.infoTables.forEach(function (infoTableRelated) {

                        let qualifiedTableName = `${infoTableRelated.table_schema}.${infoTableRelated.table_name}`;

                        if (infoTable.fkList.indexOf(qualifiedTableName) > -1) {

                            tableSeq++;
                            infoTableRelated.selectedTableSeq(tableSeq);
                            infoTableRelated.parentTableSeq(infoTable.selectedTableSeq());

                            infoTableRelated.isPrimaryQueryTable(false);
                            infoTableRelated.isSelectedQueryTable(true);
                            infoTableRelated.isJoinedQueryTable(true);
                            setCardClass(infoTableRelated);
                            selectedInfoTables.push(infoTableRelated);
                        } else {

                            //infoTableRelated.cardClass("");
                        }
                    });

                } else {

                    tableSeq++;
                    infoTable.selectedTableSeq(tableSeq);

                    infoTable.isSelectedQueryTable(true);
                    infoTable.isJoinedQueryTable(false);
                    infoTable.cardClass("flip-card-nolink");
                    selectedInfoTables.push(infoTable); // add to query
                }

                return;
            }
            function initiateNewQuery(infoTable) {

                tableSeq = 1;
                isQueryBaseEstablished = true;
                selectedInfoTables([]);

                infoTable.isPrimaryQueryTable(true);
                infoTable.isSelectedQueryTable(true);
                infoTable.isJoinedQueryTable(false);
                infoTable.selectedTableSeq(tableSeq);
                selectedInfoTables.push(infoTable);

                // select card(s) related to current card
                self.infoTables.forEach(function (infoTableRelated) {

                    let relatedNameWithSchema = `${infoTableRelated.table_schema}.${infoTableRelated.table_name}`;
                    let infoNameWithSchema = `${infoTable.table_schema}.${infoTable.table_name}`;
                    if (relatedNameWithSchema === infoNameWithSchema) {
                        return;
                    }

                    if (infoTable.fkList.indexOf(relatedNameWithSchema) > -1) {

                        tableSeq++;
                        infoTableRelated.selectedTableSeq(tableSeq);
                        infoTableRelated.parentTableSeq(infoTable.selectedTableSeq());

                        infoTableRelated.isPrimaryQueryTable(false);
                        infoTableRelated.isSelectedQueryTable(true);
                        infoTableRelated.isJoinedQueryTable(true);
                        setCardClass(infoTableRelated);

                        selectedInfoTables.push(infoTableRelated);
                    } else {

                        setCardClass(infoTableRelated);
                    }
                });

                setCardClass(infoTable); // select card that was clicked
            }
        }
        infoViewModel = new InfoTablesViewModel(allInfoTables, selectedInfoTables);
        ko.applyBindings(infoViewModel, document.getElementById("nav-info-partial"));

    });

    //---------------------------------------------------------------------------------------------query-builder view
    let queryViewModel = undefined;
    selectRequired("#nav-query-tab").on("click", function (evt) {

        if (selectedInfoTables().length === 0) {
            alert("pick a table to get started");
            return false;
        }
        location.hash = "query";
        if (queryViewModel) {

            if (isNewQuery) {

                queryViewModel.setQueryView(selectedInfoTables, isNewQuery);
                queryViewModel.onRefreshSQL();
                queryViewModel.loadTablesData();
            }

            $("table").resize(); // do not remove ... maintains column/header width

            return;
        } 

        // ko viewmodel
        function QueryViewModel( selectedInfoTables ) {

            let self = this;

            // property binding
            self.isNewMode = ko.observable(true);
            self.queryTables = ko.observableArray();
            self.queryId = undefined;
            self.needsRefresh = ko.observable(false);
            // form input
            self.queryName = setupFormEdit("");
            self.queryName.subscribe(function (newValue) {
                self.isNewMode(true);
            });
            self.rowsExpectedMax = setupFormEdit(100);
            self.rowsExpectedMax.subscribe(function (newValue) { 

            });
            self.sqlSelectStatement = setupFormEdit("");
            // dom event binding
            self.onSaveNamedQuery = function () {

                if (self.queryName() === "") {
                    self.queryName.hasError(true);
                    self.queryName.errorText("provide a unique query-name");

                    return;
                }
                isNewQuery = false; // no longer a new query

                let bodyData = { queryId: self.queryId, queryName: self.queryName(), queryTableBase: selectedInfoTables()[0].querySchemaPlusTable, querySql: self.sqlSelectStatement(), queryRowsExpected: self.rowsExpectedMax() };

                if (!self.isNewMode()) {

                    fetchJson("/api/Queries/UpdateNamedQuery", "PUT", bodyData, function (jsonResponse, response) {
                        if (response.status !== 200) {
                            self.queryName.hasError(true);
                            self.queryName.errorText(jsonResponse.appErrorText);
                        } else {
                            self.queryName.hasError(false);
                        }
                    });

                } else {

                    fetchJson("/api/Queries/InsertNamedQuery", "POST", bodyData, function (jsonResponse, response) {
                        if (response.status !== 200) {
                            self.queryName.hasError(true);
                            self.queryName.errorText(jsonResponse.appErrorText);
                        } else {
                            self.queryName.hasError(false);
                            self.queryId = jsonResponse.result;
                        }
                        self.isNewMode(self.queryName.hasError());
                    });
                }
            };
            self.onRefreshSQL = function() {

                let bodyData = [];
                ko.utils.arrayForEach(self.queryTables(), function (qt) {
                    if (qt.excludeFromQuery() === false) {
                        let columnsArray = qt.columnList.filter((el) => !qt.columnListX.includes(el)); // isolate the desired column(s)
                        bodyData.push({ JoinOn: qt.queryConditions(), IsBaseTable: qt.isPrimaryQueryTable(), TableName: qt.querySchemaPlusTable, IncludeColumns: columnsArray, FkTableNames: qt.fkList, PkColumnNames: qt.pkNames });
                    }
                });

                fetchJson("/api/Queries/MakeSqlQueryString", "POST", bodyData, function (jsonResponse, response) {

                    if (response.ok) {
                        self.sqlSelectStatement(jsonResponse.fullSql);
                        self.needsRefresh(false);
                        self.sqlSelectStatement.hasError(false);
                    } else {
                        self.sqlSelectStatement.hasError(true);
                        self.sqlSelectStatement.errorText("sql statement failed -- check syntax of each table conditions");
                    }
                });

            };
            self.onGetResult = function () {

                let bodyData = { sqlQueryStatement: self.sqlSelectStatement(), rowsExpectedMax: self.rowsExpectedMax() };
                fetchJson("/api/Queries/RunSqlQuery", "PUT", bodyData, function (jsonResponse, response) {

                    if (!response.ok) {

                        self.rowsExpectedMax.hasError(true);
                        self.rowsExpectedMax.errorText("query may have timed out.. try again");                        

                    } else {

                        if (jsonResponse.queryResult.length > self.rowsExpectedMax()) {

                            self.rowsExpectedMax.hasError(true);
                            self.rowsExpectedMax.errorText("query returns more than expected");

                        } else {

                            self.rowsExpectedMax.hasError(false);

                        }
                        console.log(jsonResponse);
                    }
                });

            };
            //
            self.setQueryView = function (selectedTables) {

                self.queryTables([]);
                self.queryName("");

                ko.utils.arrayForEach(selectedTables(), function (queryTable) {

                    queryTable.queryTable = queryTable.table_name;
                    queryTable.querySchemaPlusTable = queryTable.table_schema + "." + queryTable.table_name;
                    queryTable.queryId = queryTable.table_name;
                    queryTable.queryTitle = queryTable.querySchemaPlusTable + " " + (queryTable.isPrimaryQueryTable() ? "(primary)" : "(include/join)");
                    queryTable.queryConditions = setupFormEdit(""); //todo: something better than this
                    queryTable.queryConditions.subscribe(function (newValue) {
                        self.needsRefresh(true);
                    });

                    self.queryTables.push(queryTable);
                });
            };
            self.loadTablesData = function () {

                const cssClassExclude = "cell-header-excluded"; // see usage below

                ko.utils.arrayForEach( self.queryTables(), function ( queryTable ) {

                    let dataColumns = [];

                    queryTable.columnList.forEach(function (columnName) {
                        dataColumns.push({ data: columnName, title: columnName, class: "cell-nowrap" });
                    });                    

                    let dataTable = $("table#" + queryTable.queryId).DataTable({
                        destroy: true, paging: false, searching: false, info: false, ordering: false, scrollX: true, autoWidth: true, responsive: true,
                        ajax: {
                            url: `/api/Queries/${queryTable.querySchemaPlusTable}`,
                            data: { isPrimary: queryTable.isPrimaryQueryTable() },
                            dataSrc: ""
                        },
                        columns: dataColumns
                    });

                    // click column header to remove it from overall projection
                    let headers = dataTable.columns().header().toArray();
                    $(headers).on('click', function (evt) {

                        evt.preventDefault();

                        self.needsRefresh(true);

                        //let foo = dataTable.column($(this).attr('data-column'));
                        let columnIndex = dataTable.column(this).index();
                        let clickedColumn = dataColumns[columnIndex];

                        let columnHeaderElem = dataTable.column($(this)).header();
                        if (columnHeaderElem.classList.contains(cssClassExclude)) {
                            columnHeaderElem.classList.remove(cssClassExclude);
                            queryTable.columnListX.pop(clickedColumn.data);
                        } else {
                            columnHeaderElem.classList.add(cssClassExclude);
                            queryTable.columnListX.push(clickedColumn.data);
                        }

                        dataTable.draw();
                    });

                    // click data/cell to set selection for this table
                    dataTable.on("click", "td", function (row) {

                        self.needsRefresh(true);

                        let cellData = dataTable.cell(this).data();
                        let cellX = dataTable.cell(this).index().column;
                        let cellName = headers[cellX].innerHTML;

                        cellName = "*." + cellName; // see c# fixupCondition

                        let conditions = queryTable.queryConditions();
                        if (conditions === "") {
                            queryTable.queryConditions( `${cellName} = '${cellData}'` );
                        } else {
                            queryTable.queryConditions(`${conditions} & ${cellName} = '${cellData}'` );
                        }
                    });

                });
            };

            self.setQueryView(selectedInfoTables, isNewQuery); //
        }
        queryViewModel = new QueryViewModel( selectedInfoTables );
        ko.applyBindings( queryViewModel, document.getElementById("nav-query-partial"));

        queryViewModel.onRefreshSQL();
        queryViewModel.loadTablesData();

        $("table").resize(); // do not remove ... maintains column/header width
    });

    //---------------------------------------------------------------------------------------------get-the-code view
    let codeViewModel = undefined;
    selectRequired("#nav-code-tab").on("click", function (evt, queryName) {

        if (!queryName) {
            if (!queryViewModel || queryViewModel.isNewMode()) {
                alert("name and save your query before viewing code");
                return false;
            }
            if (codeViewModel) {
                codeViewModel.setQueryName(queryViewModel.queryName());
                return;
            }
        } else {
            if (codeViewModel) {
                codeViewModel.setQueryName(queryName);
                return;
            }
        }

        location.hash = "code";

        // ko viewmodel
        function CodeViewModel() {

            let self = this;

            self.queryName = ko.observable("");
            self.queryColumns = ko.observableArray([]);
            self.queryTypes = ko.observableArray([]);

            // bound methods
            self.runFooBarQuery = function() {

                fetchJson("/api/foobars?queryName=" + self.queryName(), "GET", function (jsonResponse, response) {
                    console.log(jsonResponse);
                });
            };
            self.setQueryName = function(queryName) {

                self.queryName(queryName);

                fetchJson("/api/Queries/GetQuerySchema?namedQuery=" + self.queryName(), "GET", function (jsonResponse, response) {

                    if (response.ok) {

                        self.queryColumns(jsonResponse.querySchema.queryColumns);
                        self.queryTypes(jsonResponse.querySchema.queryDataTypes);
                    }
                });
            };
            self.getJsonFormat = function(propertyName) {
                return propertyName.charAt(0).toLowerCase() + propertyName.slice(1);
            };
        }
        codeViewModel = new CodeViewModel();
        codeViewModel.setQueryName(!queryName ? queryViewModel.queryName() : queryName);
        ko.applyBindings(codeViewModel, document.getElementById( "nav-code-partial" ));

    });

    //---------------------------------------------------------------------------------------------query library view
    let libViewModel = undefined;
    selectRequired("#nav-lib-tab").on("click", function (evt) {

        location.hash = "library";

        if (libViewModel) return; // already been here? nothing to do?

        // ko viewmodel
        function LibViewModel(selectedInfoTables) {

            let self = this;

            // bound properties
            self.queries = ko.observableArray([]);

            // bound event handlers
            self.onSaveQueryClick = function (evt) {

                let bodyData = { queryId: evt.QueryId ,QueryName: evt.QueryName, queryRowsExpected: evt.QueryRowsExpected, QuerySql: evt.QuerySql};

                fetchJson("/api/Queries/UpdateSavedQuery", "PUT", bodyData, function (jsonResponse, response) {
                    if (response.status !== 200) {
                        //todo: ???
                    } else {
                        //todo: ???
                    }
                });

            };
            self.onRefreshClick = function () {

                self.loadAllQueries();

            };
            self.onSqlClick = function (evt) {

                let fooo = evt.isExpanded();
                ko.utils.arrayForEach(self.queries(), function (query) {

                    query.isExpanded(false);

                });
                evt.isExpanded(!fooo);
            };
            self.onRunSqlClick = function (evt) {

                fetchJson("/api/foobars?queryName=" + evt.QueryName, "GET", function (jsonResponse, response) {

                    if (response.ok) {

                        var g = JSON.stringify(jsonResponse).replace(/[\[\]\,\"]/g, ''); //stringify and remove all "stringification" extra data

                        evt.rowCountActual(jsonResponse.length);
                        evt.rowCountActual.hasError(jsonResponse.length > evt.rowCountExpected());
                        evt.rowCountActual.errorText("query returns more rows than expected");

                        evt.rowSize(g.length);

                        if (response.ok) {
                            console.log(jsonResponse);
                        }
                    }
                });

            };
            self.onSeeCodeClick = function (evt) {

                $("#nav-code-tab").trigger("click", evt.QueryName);
            };

            // bound methods
            self.loadAllQueries = function () {

                fetchJson("api/queries", "GET", function (jsonResponse, response) {

                    if (response.ok) {

                        // fixup response data with our UI needs
                        jsonResponse.forEach(function (query) {
                            query.isExpanded = ko.observable(false);
                            query.rowCountExpected = ko.observable(query.QueryRowsExpected);
                            query.rowCountActual = setupFormEdit(0);
                            query.rowSize = ko.observable(0);
                            query.elapseTime = ko.observable(0);
                        });

                        // update view with new data
                        self.queries(jsonResponse);
                    }
                });

            };

            // private
            function collapseAll(queries) {

                queries.forEach(function (query) {
                    query.isExpanded = ko.observable(false);
                });
            }
        }
        libViewModel = new LibViewModel(selectedInfoTables);
        libViewModel.loadAllQueries();
        ko.applyBindings(libViewModel, document.getElementById("nav-lib-partial"));

    });


    //---------------------------------------------------------------------------------------------useful stuff
    function selectRequired(selector) {

        let wrappedSet = $(selector);

        if (wrappedSet.length === 0) {
            alert(`programming issue.. required selector [${selector}] returned zero`);
            throw "jQuery selector not found in dom";
        }

        return $(wrappedSet);
    }
    function setupFormEdit(initialValue) {

        let formField = ko.observable(initialValue);

        formField.hasError = ko.observable(false);
        formField.errorText = ko.observable("");

        return formField;
    }
    function fetchJson(url, method, callBackOrBody, callBackOrUndefined) {

        let options = {};
        options.method = method;
        options.headers = { "accept": "application/json, text/plain, */*", "content-type": "application/json" };
        // optional body paramater
        options.body = callBackOrUndefined ? JSON.stringify(callBackOrBody) : callBackOrUndefined;

        let serverResponse;

        fetch(url, options)
        .then(response => {
            serverResponse = response;
            return response.json();
        })
        .then(jsonResponse => {
            callBackOrUndefined ? callBackOrUndefined(jsonResponse, serverResponse) : callBackOrBody(jsonResponse, serverResponse);
        })
        .catch(function (err) { console.log(err); alert("fetch went wroong.. see console log"); });
    
    }

});

